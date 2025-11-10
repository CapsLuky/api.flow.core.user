using Core.ClerkWebhook.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class ClerkWebhookEndpoints
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        var webhooks = endpoints.MapGroup("/api/webhooks")
            .WithTags("Clerk Webhooks")
            .WithOpenApi();
        
        webhooks.MapPost("/clerk", ProcessClerkWebhook)
            .WithName("ProcessClerkWebhook")
            .WithSummary("Processar webhook do Clerk")
            .WithDescription("Endpoint para receber e processar webhooks do Clerk via Svix")
            .Produces<string>(200)
            .Produces<string>(400)
            .Produces<string>(500);
        
        return endpoints;
    }
    
    
    private static async Task<IResult> ProcessClerkWebhook(
        HttpContext httpContext,
        [FromServices] IClerkWebhookService clerkWebhookService,
        [FromServices] ILogger<ClerkWebhookEndpointsLoggerMarker> logger)
    {
        try
        {
            var success = await clerkWebhookService.ProcessWebhookAsync(
                httpContext.Request,
                httpContext.RequestAborted);
            
            return success 
                ? Results.Ok(new { message = "Webhook processado com sucesso", timestamp = DateTime.UtcNow })
                : Results.BadRequest(new { error = "Falha ao processar webhook", timestamp = DateTime.UtcNow });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Processamento do webhook foi cancelado");
            return Results.StatusCode(499); // Client Closed Request
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Erro interno ao processar webhook do Clerk");
            // Log específico seria feito no serviço
            return Results.Problem(
                title: "Erro interno do servidor",
                detail: "Erro ao processar webhook do Clerk",
                statusCode: 500);
        }
    }
}