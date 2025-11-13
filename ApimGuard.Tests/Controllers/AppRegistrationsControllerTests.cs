using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
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
    private readonly AppRegistrationsController _controller;

    public AppRegistrationsControllerTests()
    {
        _mockLogger = new Mock<ILogger<AppRegistrationsController>>();
        _mockGraphApiService = new Mock<IGraphApiService>();
        _controller = new AppRegistrationsController(_mockLogger.Object, _mockGraphApiService.Object);
        
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
}
