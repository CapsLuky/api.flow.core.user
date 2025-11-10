using Core.ClerkWebhook.Models;

namespace Core.ClerkWebhook.Abstractions;

public interface IClerkWebhookRepository
{
    Task<bool> ProcessUserCreatedAsync(ClerkUserData userData, ClerkApplication clerkApplication,
        CancellationToken cancellationToken = default);

    Task<bool> ProcessUserUpdatedAsync(ClerkUserData userData, ClerkApplication clerkApplication,
        CancellationToken cancellationToken = default);

    Task<bool> ProcessUserDeletedAsync(ClerkDeletedUserData userData, ClerkApplication clerkApplication,
        CancellationToken cancellationToken = default);
}