using Core.ClerkWebhook.Services;
using Core.Client.Models;
using Core.Client.Services;
using Core.Client.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class DependencyInjection
{
    public static void AddCore(this IServiceCollection services)
    {
        services
            .AddClerkServices()
            .AddValidatorsServices();
    }
    
    private static IServiceCollection AddClerkServices(this IServiceCollection services)
        => services
            .AddScoped<IClerkWebhookService, ClerkWebhookService>()
            .AddScoped<IUserService, UserService>();
    
    private static IServiceCollection AddValidatorsServices(this IServiceCollection services)
        => services
            .AddTransient<IValidator<User>, UserValidator>();

}