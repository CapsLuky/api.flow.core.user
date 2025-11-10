using System.Text;
using System.Text.Json;
using Core.ClerkWebhook.Abstractions;
using Core.ClerkWebhook.Models;
using Core.Generic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using Svix;

namespace Core.ClerkWebhook.Services;

public class ClerkWebhookService(
    ILogger<ClerkWebhookService> logger,
    ClerkOptions clerkOptions,
    IClerkWebhookRepository clerkWebhookRepository) : IClerkWebhookService
{

    public async Task<bool> ProcessWebhookAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ler o corpo da requisição como string RAW
            request.EnableBuffering();
            string requestBody;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync(cancellationToken); 
            }

            request.Body.Position = 0;

            if (string.IsNullOrEmpty(requestBody))
            {
                logger.LogWarning("Webhook payload está vazio");
                return false;
            }

            // Obter os headers necessários para verificação
            var headers = new System.Net.WebHeaderCollection();

            request.Headers.TryGetValue("application_id", out var applicationId);
            if (string.IsNullOrEmpty(applicationId.ToString()))
            {
                logger.LogWarning("Header inválido evento application_id ausente");
                return false;
            }

            if (request.Headers.TryGetValue("svix-id", out var svixId))
                headers.Add("svix-id", svixId.ToString());

            if (request.Headers.TryGetValue("svix-timestamp", out var svixTimestamp))
                headers.Add("svix-timestamp", svixTimestamp.ToString());

            if (request.Headers.TryGetValue("svix-signature", out var svixSignature))
                headers.Add("svix-signature", svixSignature.ToString());

            // Verificar se todos os headers necessários estão presentes
            if (string.IsNullOrEmpty(headers.Get("svix-id")) || 
                string.IsNullOrEmpty(headers.Get("svix-timestamp")) || 
                string.IsNullOrEmpty(headers.Get("svix-signature")))
            {
                logger.LogWarning("Headers necessários do Svix não encontrados");
                return false;
            }

            // Determinar a aplicação e o secret
            var clerkApplication = ClerkApplication.MultiTenant;
            string secret;

            if (applicationId.ToString() == "comgas")
            {
                clerkApplication = ClerkApplication.Comgas;
                secret = clerkOptions.WebhookSecretComgas;
            }
            else
            {
                secret = clerkOptions.WebhookSecret;
            }

            if (string.IsNullOrEmpty(secret))
            {
                logger.LogError("Webhook secret para a aplicação '{ApplicationId}' não está configurado.", applicationId.ToString());
                return false;
            }

            var svixWebhook = new Webhook(secret);

            // Verificar a assinatura do webhook usando Svix
            try
            {
                svixWebhook.Verify(requestBody, headers);
                logger.LogInformation("Webhook verificado com sucesso para o ApplicationId: {ApplicationId}", applicationId.ToString());

                return await ProcessVerifiedPayload(requestBody, clerkApplication, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[ProcessWebhookAsync] Erro ao verificar assinatura webhook: {ExMessage}", ex.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ProcessWebhookAsync] Erro ao processar webhook: {ExMessage}", ex.Message);
            return false;
        }
    }

    private async Task<bool> ProcessVerifiedPayload(string verifiedPayload, ClerkApplication clerkApplication, CancellationToken cancellationToken)
    {
        try
        {
            // Deserializar o payload verificado
            var webhookEvent = JsonSerializer.Deserialize<ClerkWebhookEvent>(verifiedPayload);
            if (webhookEvent?.Data == null)
            {
                logger.LogWarning("Payload do evento inválido");
                return false;
            }

            logger.LogInformation("Processando evento webhook: {EventType}", webhookEvent.Type);

            // Processar o evento com base no seu tipo
            return webhookEvent.Type switch
            {
                ClerkWebhookEvents.UserCreated => await ProcessUserCreatedAsync(webhookEvent, clerkApplication, cancellationToken),
                ClerkWebhookEvents.UserUpdated => await ProcessUserUpdatedAsync(webhookEvent, clerkApplication, cancellationToken),
                ClerkWebhookEvents.UserDeleted => await ProcessUserDeletedAsync(webhookEvent, clerkApplication, cancellationToken),
                _ => await ProcessUnknownEventAsync(webhookEvent.Type, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ProcessVerifiedPayload] Erro ao processar payload verificado: {ExMessage}", ex.Message);
            return false;
        }
    }

    public async Task<bool> ProcessUserCreatedAsync(ClerkWebhookEvent webhookEvent, ClerkApplication clerkApplication, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonPayload = webhookEvent.Data?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(jsonPayload))
            {
                logger.LogWarning("Payload de dados do usuário criado está vazio.");
                return false;
            }

            // 1. Desserializa o payload com BsonSerializer.
            //    Isso converte corretamente todos os tipos, mas deixará ClerkId nulo
            //    porque o BsonSerializer procura por "ClerkId" no JSON, não por "id".
            var userData = BsonSerializer.Deserialize<ClerkUserData>(jsonPayload);
            
            // 2. Extrai manualmente o "id" do JsonElement original e o atribui à propriedade ClerkId.
            //    Isso completa o objeto userData para que ele possa ser usado pelo repositório.
            if (webhookEvent.Data != null && webhookEvent.Data.Value.TryGetProperty("id", out var idElement))
            {
                userData.ClerkId = idElement.GetString();
            }

            if (string.IsNullOrEmpty(userData.ClerkId))
            {
                logger.LogWarning("Não foi possível extrair o ClerkId do payload do usuário criado.");
                return false;
            }

            logger.LogInformation("Iniciando persistência de usuário ClerkId: {ClerkId}", userData.ClerkId);

            var success = await clerkWebhookRepository.ProcessUserCreatedAsync(userData, clerkApplication, cancellationToken);

            if (success)
            {
                logger.LogInformation("Persistência do usuário ClerkId {ClerkId} concluída com sucesso.", userData.ClerkId);
            }
            else
            {
                logger.LogError("Falha ao persistir usuário ClerkId {ClerkId} no repositório.", userData.ClerkId);
            }

            return success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao processar criação de usuário. {ExMessage}", ex.Message);
            return false;
        }
    }

    public async Task<bool> ProcessUserUpdatedAsync(ClerkWebhookEvent webhookEvent, ClerkApplication clerkApplication, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonPayload = webhookEvent.Data?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(jsonPayload))
            {
                logger.LogWarning("Payload de dados do usuário atualizado está vazio.");
                return false;
            }

            // 1. Desserializa o payload com BsonSerializer.
            //    Isso converte todos os tipos corretamente, mas deixará ClerkId nulo.
            var userData = BsonSerializer.Deserialize<ClerkUserData>(jsonPayload);
            
            // 2. Extrai manualmente o "id" do JsonElement e o atribui a ClerkId.
            if (webhookEvent.Data != null && webhookEvent.Data.Value.TryGetProperty("id", out var idElement))
            {
                userData.ClerkId = idElement.GetString();
            }

            if (string.IsNullOrEmpty(userData.ClerkId))
            {
                logger.LogWarning("Não foi possível extrair o ClerkId do payload de atualização.");
                return false;
            }
            
            logger.LogInformation("Iniciando atualização de usuário com ClerkId: {ClerkId}", userData.ClerkId);

            return await clerkWebhookRepository.ProcessUserUpdatedAsync(userData, clerkApplication, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar atualização de usuário. {ExMessage}", ex.Message);
            return false;
        }
    }

    public async Task<bool> ProcessUserDeletedAsync(ClerkWebhookEvent webhookEvent, ClerkApplication clerkApplication, CancellationToken cancellationToken = default)
    {
        try
        {
            // Para a exclusão, só precisamos do ID, então o modelo simples é suficiente.
            var userData = JsonSerializer.Deserialize<ClerkDeletedUserData>(webhookEvent.Data?.ToString() ?? string.Empty);
            if (userData?.Id == null)
            {
                logger.LogWarning("Falha ao deserializar dados do usuário deletado ou ID é nulo.");
                return false;
            }
            
            logger.LogInformation("Iniciando exclusão de usuário: {UserId}", userData.Id);
            return await clerkWebhookRepository.ProcessUserDeletedAsync(userData, clerkApplication, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar exclusão de usuário. {ExMessage}", ex.Message);
            return false;
        }
    }

    private Task<bool> ProcessUnknownEventAsync(string? eventType, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Tipo de evento desconhecido recebido: {EventType}", eventType);
        return Task.FromResult(true); // Retornar true para evitar retry de eventos desconhecidos
    }
}