namespace Core.Generic.Models;

public class ClerkOptions
{
    public const string Clerk = "Clerk";

    public string WebhookSecret { get; set; } = string.Empty;
    public string WebhookSecretComgas { get; set; } = string.Empty;
    public int WebhookRetryAttempts { get; set; }
    public int WebhookTimeout { get; set; }
}
