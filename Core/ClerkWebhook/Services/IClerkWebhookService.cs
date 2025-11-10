using System.Text.Json;
using Core.ClerkWebhook.Models;
using Microsoft.AspNetCore.Http;

namespace Core.ClerkWebhook.Services;

public interface IClerkWebhookService
{
    Task<bool> ProcessWebhookAsync(HttpRequest request, CancellationToken cancellationToken = default);
    Task<bool> ProcessUserCreatedAsync(ClerkWebhookEvent webhookEvent, ClerkApplication clerkApplication, CancellationToken cancellationToken = default);
    Task<bool> ProcessUserUpdatedAsync(ClerkWebhookEvent webhookEvent, ClerkApplication clerkApplication, CancellationToken cancellationToken = default);
    Task<bool> ProcessUserDeletedAsync(ClerkWebhookEvent webhookEvent, ClerkApplication clerkApplication, CancellationToken cancellationToken = default);
 
}