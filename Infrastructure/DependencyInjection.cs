using Core.ClerkWebhook.Abstractions;
using Core.Client.Abstrations;
using Core.Generic.Models;
using Infrastructure.Client.Repository;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.ClerkWebhook.Repository;
using Infrastructure.Statics;
using MongoDB.Driver;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddIntegrations()
            .AddRepositories()
            .AddExternalServices();
    }
    
    /// <summary>
    /// Registrar repositórios aqui
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Registrar MongoDB como singleton
        services.AddSingleton<IMongoClient>(provider =>
        {
            var mongoUrl = provider.GetRequiredService<EnvironmentMongoUrl>();
            return new MongoClient(mongoUrl.UrlMongoDb);
        });

        services.AddSingleton<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(Database.MongoDbUserAccount);
        });

        
        return services;
    }
    
    /// <summary>
    /// Registrar serviços externos aqui
    /// </summary>
    private static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        return services;
    }
    
    private static IServiceCollection AddIntegrations(this IServiceCollection services)
    {
        return services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IClerkWebhookRepository, ClerkWebhookRepository >();
    }
}