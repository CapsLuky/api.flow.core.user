using Core.Generic.Models;
using Microsoft.Extensions.Options;

namespace Api;

public static class DependencyInjection
{
    public static void AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddEndpoints()
            .AddEnvironmentConfigurations(configuration);
    }

    private static IServiceCollection AddEndpoints(this IServiceCollection services)
        => services
            .AddEndpointsApiExplorer();
    
    private static IServiceCollection AddEnvironmentConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // Configura o Options Pattern para ClerkOptions
        services.Configure<ClerkOptions>(configuration.GetSection(ClerkOptions.Clerk));
        
        // Registra a instância de ClerkOptions como singleton para injeção direta
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ClerkOptions>>().Value);

        return services
            .AddSingleton(new EnvironmentMongoUrl(configuration.GetConnectionString("MongoDB") ?? String.Empty));
    }

}