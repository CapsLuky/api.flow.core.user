namespace Core.ClerkWebhook.Models;

public static class ClerkWebhookEvents
{
    public const string UserCreated = "user.created";
    public const string UserUpdated = "user.updated";
    public const string UserDeleted = "user.deleted";
    public const string SessionCreated = "session.created";
    public const string SessionEnded = "session.ended";
    public const string SessionRevoked = "session.revoked";
    public const string EmailCreated = "email.created";
    public const string OrganizationCreated = "organization.created";
    public const string OrganizationUpdated = "organization.updated";
    public const string OrganizationDeleted = "organization.deleted";
    public const string OrganizationMembershipCreated = "organizationMembership.created";
    public const string OrganizationMembershipUpdated = "organizationMembership.updated";
    public const string OrganizationMembershipDeleted = "organizationMembership.deleted";
    public const string OrganizationInvitationCreated = "organizationInvitation.created";
    public const string OrganizationInvitationAccepted = "organizationInvitation.accepted";
    public const string OrganizationInvitationRevoked = "organizationInvitation.revoked";
}