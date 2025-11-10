using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Core.ClerkWebhook.Services;
using Core.ClerkWebhook.Abstractions;
using Core.Generic.Models;
using Core.ClerkWebhook.Models;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FluentAssertions;


namespace Unit.Test.Core.ClerkWebhook;

public class ClerkWebhookServiceTest
{
    private readonly Mock<ILogger<ClerkWebhookService>> _loggerMock;
    private readonly Mock<IClerkWebhookRepository> _clerkWebhookRepositoryMock;
    private readonly ClerkWebhookService _sut; // SUT = System Under Test

    public ClerkWebhookServiceTest()
    {
        _loggerMock = new Mock<ILogger<ClerkWebhookService>>();
        var clerkOptions = new ClerkOptions
        {
            WebhookSecret = "whsec_test_secret_multitenant",
            WebhookSecretComgas = "whsec_test_secret_comgas"
        };

        _clerkWebhookRepositoryMock = new Mock<IClerkWebhookRepository>();
        _sut = new ClerkWebhookService(_loggerMock.Object, clerkOptions, _clerkWebhookRepositoryMock.Object);
    }
    
    private static HttpRequest CreateMockRequest(string? body, Dictionary<string, string>? headers = null)
    {
        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;
    
        if (!string.IsNullOrEmpty(body))
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            request.Body = stream;
            request.ContentLength = stream.Length;
        }
    
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers[header.Key] = header.Value;
            }
        }
        
        return request;
    }

    #region ProcessUserCreatedAsync

    [Fact]
    public async Task ProcessUserCreatedAsync_WithValidData_ShouldCallRepositoryAndReturnTrue()
    {
        // Arrange (Preparação)
        var clerkApplication = ClerkApplication.MultiTenant;
        var userData = new ClerkUserData { Id = "user_12345", ClerkId = "clerk_abcde" };
        var webhookEvent = new ClerkWebhookEvent
        {
            Type = ClerkWebhookEvents.UserCreated,
            Data = JsonSerializer.SerializeToElement(userData)
        };
        
        _clerkWebhookRepositoryMock
            .Setup(repo => repo.ProcessUserCreatedAsync(
                It.IsAny<ClerkUserData>(), 
                clerkApplication, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    
        // Act (Ação)
        var result = await _sut.ProcessUserCreatedAsync(webhookEvent, clerkApplication);
    
        // Assert (Verificação)
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task ProcessUserCreatedAsync_WithInvalidUserData_ShouldReturnFalse()
    {
        // Arrange
        var clerkApplication = ClerkApplication.MultiTenant;
        var webhookEvent = new ClerkWebhookEvent { Type = ClerkWebhookEvents.UserCreated, Data = null };

        // Act
        var result = await _sut.ProcessUserCreatedAsync(webhookEvent, clerkApplication);

        // Assert
        result.Should().BeFalse();
        
        _clerkWebhookRepositoryMock.Verify(repo => repo.ProcessUserCreatedAsync(
                It.IsAny<ClerkUserData>(),
                It.IsAny<ClerkApplication>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
    
    #region ProcessUserDeletedAsync

    [Fact]
    public async Task ProcessUserDeletedAsync_WithValidData_ShouldCallRepositoryAndReturnTrue()
    {
        // Arrange
        var clerkApplication = ClerkApplication.MultiTenant;
        var deletedUserData = new ClerkDeletedUserData { Id = "user_abcdef", Deleted = true };
        var webhookEvent = new ClerkWebhookEvent
        {
            Type = ClerkWebhookEvents.UserDeleted,
            Data = JsonSerializer.SerializeToElement(deletedUserData)
        };
    
        _clerkWebhookRepositoryMock
            .Setup(repo => repo.ProcessUserDeletedAsync(It.IsAny<ClerkDeletedUserData>(), clerkApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    
        // Act
        var result = await _sut.ProcessUserDeletedAsync(webhookEvent, clerkApplication);
    
        // Assert
        result.Should().BeTrue();
        _clerkWebhookRepositoryMock.Verify(repo => repo.ProcessUserDeletedAsync(
                It.Is<ClerkDeletedUserData>(u => u.Id == deletedUserData.Id), 
                clerkApplication, 
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task ProcessUserDeletedAsync_WithInvalidData_ShouldReturnFalse()
    {
        // Arrange
        var clerkApplication = ClerkApplication.MultiTenant;
        var webhookEvent = new ClerkWebhookEvent { Type = ClerkWebhookEvents.UserDeleted, Data = null };
    
        // Act
        var result = await _sut.ProcessUserDeletedAsync(webhookEvent, clerkApplication);
    
        // Assert
        result.Should().BeFalse();
        _clerkWebhookRepositoryMock.Verify(repo => repo.ProcessUserDeletedAsync(
                It.IsAny<ClerkDeletedUserData>(), 
                It.IsAny<ClerkApplication>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    #endregion

    #region ProcessUserUpdatedAsync

    [Fact]
    public async Task ProcessUserUpdatedAsync_WithValidData_ShouldCallRepositoryAndReturnTrue()
    {
        // Arrange
        var clerkApplication = ClerkApplication.Comgas;
        var userData = new ClerkUserData { Id = "user_67890", ClerkId = "clerk_fghij" };
        var webhookEvent = new ClerkWebhookEvent
        {
            Type = ClerkWebhookEvents.UserUpdated,
            Data = JsonSerializer.SerializeToElement(userData)
        };
    
        _clerkWebhookRepositoryMock
            .Setup(repo => repo.ProcessUserUpdatedAsync(It.IsAny<ClerkUserData>(), clerkApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    
        // Act
        var result = await _sut.ProcessUserUpdatedAsync(webhookEvent, clerkApplication);
    
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task ProcessUserUpdatedAsync_WithInvalidData_ShouldReturnFalse()
    {
        // Arrange
        var clerkApplication = ClerkApplication.Comgas;
        var webhookEvent = new ClerkWebhookEvent { Type = ClerkWebhookEvents.UserUpdated, Data = null };
    
        // Act
        var result = await _sut.ProcessUserUpdatedAsync(webhookEvent, clerkApplication);
    
        // Assert
        result.Should().BeFalse();
        _clerkWebhookRepositoryMock.Verify(repo => repo.ProcessUserUpdatedAsync(
                It.IsAny<ClerkUserData>(), 
                It.IsAny<ClerkApplication>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }


    #endregion

    #region ProcessWebhookAsync

    [Fact]
    public async Task ProcessWebhookAsync_WithEmptyBody_ShouldReturnFalse()
    {
        // Arrange
        var request = CreateMockRequest(string.Empty);
    
        // Act
        var result = await _sut.ProcessWebhookAsync(request);
    
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task ProcessWebhookAsync_WithMissingSvixHeaders_ShouldReturnFalse()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            { "application_id", "comgas" }
        };
        var request = CreateMockRequest("{ 'key': 'value' }", headers);
    
        // Act
        var result = await _sut.ProcessWebhookAsync(request);
    
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task ProcessWebhookAsync_WithMissingSecret_ShouldReturnFalse()
    {
        // Arrange
        var optionsWithoutSecret = new ClerkOptions { WebhookSecretComgas = "" };
        var serviceWithBadConfig = new ClerkWebhookService(
            _loggerMock.Object, 
            optionsWithoutSecret, 
            _clerkWebhookRepositoryMock.Object);
            
        var headers = new Dictionary<string, string>
        {
            { "application_id", "comgas" },
            { "svix-id", "test_id" },
            { "svix-timestamp", "12345678" },
            { "svix-signature", "v1,test_signature" }
        };
        var request = CreateMockRequest("{ 'key': 'value' }", headers);
    
        // Act
        var result = await serviceWithBadConfig.ProcessWebhookAsync(request);
    
        // Assert
        result.Should().BeFalse();
    }

    #endregion

}
