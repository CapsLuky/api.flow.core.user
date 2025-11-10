using Core.ClerkWebhook.Abstractions;
using Core.ClerkWebhook.Models;
using Core.Generic.Models;
using Infrastructure.Statics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.ClerkWebhook.Repository;

public class ClerkWebhookRepository(
    ILogger<ClerkWebhookRepository> logger,
    IMongoDatabase database
) : IClerkWebhookRepository
{
    private IMongoCollection<ClerkUserData> GetUsersCollection(ClerkApplication clerkApplication)
    {
        var collectionName = clerkApplication switch
        {
            ClerkApplication.Comgas => Collection.UsersComgas,
            _ => Collection.Users
        };
        return database.GetCollection<ClerkUserData>(collectionName);
    }

    public async Task<bool> ProcessUserCreatedAsync(ClerkUserData userData, ClerkApplication clerkApplication, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetUsersCollection(clerkApplication);
            await collection.InsertOneAsync(userData, null, cancellationToken);
            logger.LogInformation("Usuário {UserId} persistido com sucesso na collection {CollectionName}.", userData.Id, collection.CollectionNamespace.CollectionName);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogWarning("Tentativa de inserir usuário duplicado com ID: {UserId}. O usuário já existe.", userData.Id);
            return true; // Retorna true porque o estado desejado (usuário existe) foi alcançado.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao persistir usuário {UserId} no banco de dados.", userData.Id);
            return false;
        }
    }

    public async Task<bool> ProcessUserUpdatedAsync(ClerkUserData userData, ClerkApplication clerkApplication, CancellationToken cancellationToken = default)
    {
        try
        {
            var userCollection = GetUsersCollection(clerkApplication);

            var filter = Builders<ClerkUserData>
                .Filter.Eq(u => u.ClerkId, userData.ClerkId);

            var update = Builders<ClerkUserData>.Update
                .Set(u => u.FirstName, userData.FirstName)
                .Set(u => u.LastName, userData.LastName)
                .Set(u => u.ImageUrl, userData.ImageUrl)
                .Set(u => u.EmailAddresses, userData.EmailAddresses)
                .Set(u => u.PhoneNumbers, userData.PhoneNumbers)
                .Set(u => u.Username, userData.Username)
                .Set(u => u.UpdatedAt, userData.UpdatedAt)
                .Set(u => u.LastSignInAt, userData.LastSignInAt)
                .Set(u => u.ExternalId, userData.ExternalId)
                .Set(u => u.PrimaryEmailAddressId, userData.PrimaryEmailAddressId)
                .Set(u => u.PublicMetadata, userData.PublicMetadata)
                .Set(u => u.PrivateMetadata, userData.PrivateMetadata)
                .Set(u => u.UnsafeMetadata, userData.UnsafeMetadata);

            var result = await userCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
            {
                logger.LogWarning(
                    "Nenhum usuário encontrado para update com ClerkId={ClerkId} na collection {CollectionName}.",
                    userData.ClerkId, userCollection.CollectionNamespace.CollectionName);
            }
            else if (result.ModifiedCount > 0)
            {
                logger.LogInformation(
                    "Usuário com ClerkId={ClerkId} atualizado com sucesso na collection {CollectionName}.",
                    userData.ClerkId, userCollection.CollectionNamespace.CollectionName);
            }
            else
            {
                logger.LogInformation(
                    "Usuário com ClerkId={ClerkId} não teve alterações (já estava atualizado) na collection {CollectionName}.",
                    userData.ClerkId, userCollection.CollectionNamespace.CollectionName);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao atualizar usuário com ClerkId={ClerkId}.",
                userData.ClerkId);
            return false;
        }
    }

    public async Task<bool> ProcessUserDeletedAsync(ClerkDeletedUserData userData, ClerkApplication clerkApplication, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetUsersCollection(clerkApplication);
            var filter = Builders<ClerkUserData>.Filter.Eq(u => u.ClerkId, userData.Id);
            var result = await collection.DeleteOneAsync(filter, cancellationToken);

            if (result.DeletedCount > 0)
            {
                logger.LogInformation(
                    "Usuário com ClerkId={ClerkId} excluído com sucesso da collection {CollectionName}.",
                    userData.Id,
                    collection.CollectionNamespace.CollectionName
                );
            }
            else
            {
                logger.LogWarning(
                    "Nenhum usuário encontrado para exclusão com ClerkId={ClerkId} na collection {CollectionName}.",
                    userData.Id,
                    collection.CollectionNamespace.CollectionName
                );
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao excluir usuário com ClerkId={ClerkId} do banco de dados.",
                userData.Id
            );
            return false;
        }
    }
}