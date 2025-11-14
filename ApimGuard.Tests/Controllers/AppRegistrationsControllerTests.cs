using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ApimGuard.Controllers;
using ApimGuard.Models;
using ApimGuard.Services;
using Xunit;

namespace ApimGuard.Tests.Controllers;

public class AppRegistrationsControllerTests
{
    private readonly Mock<ILogger<AppRegistrationsController>> _mockLogger;
    private readonly Mock<IGraphApiService> _mockGraphApiService;
    private readonly Mock<IOptions<FeatureFlags>> _mockFeatureFlags;
    private readonly AppRegistrationsController _controller;

    public AppRegistrationsControllerTests()
    {
        _mockLogger = new Mock<ILogger<AppRegistrationsController>>();
        _mockGraphApiService = new Mock<IGraphApiService>();
        _mockFeatureFlags = new Mock<IOptions<FeatureFlags>>();
        _mockFeatureFlags.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = true, EnableModifyOperations = true });
        _controller = new AppRegistrationsController(_mockLogger.Object, _mockGraphApiService.Object, _mockFeatureFlags.Object);
        
        // Setup TempData for the controller
        var tempData = new TempDataDictionary(
            new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Fact]
    public async Task AddSecret_StoresSecretValueInTempData_WhenSuccessful()
    {
        // Arrange
        var appId = "test-app-id";
        var displayName = "Test Secret";
        var validityMonths = 12;
        var secretValue = "test-secret-value-123";

        var mockSecret = new AppSecretInfo
        {
            Id = "secret-id",
            DisplayName = displayName,
            SecretValue = secretValue,
            StartDateTime = DateTime.UtcNow,
            EndDateTime = DateTime.UtcNow.AddMonths(validityMonths)
        };

        _mockGraphApiService
            .Setup(s => s.AddApplicationSecretAsync(appId, displayName, validityMonths))
            .ReturnsAsync(mockSecret);

        // Act
        var result = await _controller.AddSecret(appId, displayName, validityMonths);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Secrets), redirectResult.ActionName);
        Assert.Equal(appId, redirectResult.RouteValues?["id"]);
        Assert.Equal(secretValue, _controller.TempData["SecretValue"]);
        Assert.Null(_controller.TempData["Error"]);
    }

    [Fact]
    public async Task AddSecret_StoresErrorInTempData_WhenExceptionOccurs()
    {
        // Arrange
        var appId = "test-app-id";
        var displayName = "Test Secret";
        var validityMonths = 12;

        _mockGraphApiService
            .Setup(s => s.AddApplicationSecretAsync(appId, displayName, validityMonths))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.AddSecret(appId, displayName, validityMonths);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Secrets), redirectResult.ActionName);
        Assert.Equal(appId, redirectResult.RouteValues?["id"]);
        Assert.Null(_controller.TempData["SecretValue"]);
        Assert.NotNull(_controller.TempData["Error"]);
        Assert.Equal("Failed to add secret. Please try again.", _controller.TempData["Error"]);
    }

    [Fact]
    public async Task Secrets_ReturnsViewWithSecrets()
    {
        // Arrange
        var appId = "test-app-id";
        var secrets = new List<AppSecretInfo>
        {
            new AppSecretInfo { Id = "secret1", DisplayName = "Secret 1" },
            new AppSecretInfo { Id = "secret2", DisplayName = "Secret 2" }
        };
        _mockGraphApiService.Setup(s => s.GetApplicationSecretsAsync(appId)).ReturnsAsync(secrets);

        // Act
        var result = await _controller.Secrets(appId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<AppSecretInfo>>(viewResult.Model);
        Assert.Equal(2, model.Count);
        Assert.Equal(appId, viewResult.ViewData["AppId"]);
    }

    [Fact]
    public async Task DetailsByAppId_RedirectsToDetails_WhenAppIsFound()
    {
        // Arrange
        var clientAppId = "12345678-1234-1234-1234-123456789abc";
        var objectId = "object-id-123";
        var mockApp = new AppRegistrationInfo
        {
            Id = objectId,
            AppId = clientAppId,
            DisplayName = "Test App"
        };

        _mockGraphApiService
            .Setup(s => s.GetApplicationByAppIdAsync(clientAppId))
            .ReturnsAsync(mockApp);

        // Act
        var result = await _controller.DetailsByAppId(clientAppId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal(objectId, redirectResult.RouteValues?["id"]);
        Assert.Null(_controller.TempData["Error"]);
    }

    [Fact]
    public async Task DetailsByAppId_RedirectsToIndex_WhenAppNotFound()
    {
        // Arrange
        var clientAppId = "12345678-1234-1234-1234-123456789abc";

        _mockGraphApiService
            .Setup(s => s.GetApplicationByAppIdAsync(clientAppId))
            .ReturnsAsync((AppRegistrationInfo?)null);

        // Act
        var result = await _controller.DetailsByAppId(clientAppId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.NotNull(_controller.TempData["Error"]);
        Assert.Contains(clientAppId, _controller.TempData["Error"]?.ToString());
    }

    [Fact]
    public async Task DetailsByAppId_RedirectsToIndex_WhenExceptionOccurs()
    {
        // Arrange
        var clientAppId = "12345678-1234-1234-1234-123456789abc";

        _mockGraphApiService
            .Setup(s => s.GetApplicationByAppIdAsync(clientAppId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.DetailsByAppId(clientAppId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.NotNull(_controller.TempData["Error"]);
        Assert.Equal("Failed to retrieve app registration.", _controller.TempData["Error"]);
    }

    [Fact]
    public async Task Index_ReturnsOnlyAppsWithAPIMSECTag()
    {
        // Arrange
        var apps = new List<AppRegistrationInfo>
        {
            new AppRegistrationInfo 
            { 
                Id = "app1", 
                DisplayName = "App 1", 
                Tags = new List<string> { "APIMSEC" } 
            },
            new AppRegistrationInfo 
            { 
                Id = "app2", 
                DisplayName = "App 2", 
                Tags = new List<string> { "APIMSEC", "Production" } 
            }
        };
        _mockGraphApiService.Setup(s => s.GetApplicationsAsync()).ReturnsAsync(apps);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<AppRegistrationInfo>>(viewResult.Model);
        Assert.Equal(2, model.Count);
        Assert.All(model, app => Assert.Contains("APIMSEC", app.Tags));
    }

    [Fact]
    public async Task Create_CreatesAppWithAPIMSECTag()
    {
        // Arrange
        var displayName = "Test App";
        var redirectUris = new List<string> { "https://localhost" };
        var appToCreate = new AppRegistrationInfo
        {
            DisplayName = displayName,
            RedirectUris = redirectUris
        };

        var createdApp = new AppRegistrationInfo
        {
            Id = "new-app-id",
            AppId = "new-client-id",
            DisplayName = displayName,
            RedirectUris = redirectUris,
            Tags = new List<string> { "APIMSEC" }
        };

        _mockGraphApiService
            .Setup(s => s.CreateApplicationAsync(displayName, redirectUris))
            .ReturnsAsync(createdApp);

        // Act
        var result = await _controller.Create(appToCreate);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockGraphApiService.Verify(s => s.CreateApplicationAsync(displayName, redirectUris), Times.Once);
    }
}
