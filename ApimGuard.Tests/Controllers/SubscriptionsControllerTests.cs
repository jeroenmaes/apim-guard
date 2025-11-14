using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ApimGuard.Controllers;
using ApimGuard.Models;
using ApimGuard.Services;
using Xunit;

namespace ApimGuard.Tests.Controllers;

public class SubscriptionsControllerTests
{
    private readonly Mock<ILogger<SubscriptionsController>> _mockLogger;
    private readonly Mock<IApiManagementService> _mockApiManagementService;
    private readonly Mock<IOptions<FeatureFlags>> _mockFeatureFlags;
    private readonly SubscriptionsController _controller;

    public SubscriptionsControllerTests()
    {
        _mockLogger = new Mock<ILogger<SubscriptionsController>>();
        _mockApiManagementService = new Mock<IApiManagementService>();
        _mockFeatureFlags = new Mock<IOptions<FeatureFlags>>();
        _mockFeatureFlags.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = true, EnableModifyOperations = true });
        _controller = new SubscriptionsController(_mockLogger.Object, _mockApiManagementService.Object, _mockFeatureFlags.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithSubscriptions()
    {
        // Arrange
        var subscriptions = new List<SubscriptionInfo>
        {
            new SubscriptionInfo { Id = "sub1", Name = "sub1", DisplayName = "Subscription 1" },
            new SubscriptionInfo { Id = "sub2", Name = "sub2", DisplayName = "Subscription 2" }
        };
        _mockApiManagementService.Setup(s => s.GetSubscriptionsAsync()).ReturnsAsync(subscriptions);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<SubscriptionInfo>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task Index_ReturnsEmptyListOnException()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.GetSubscriptionsAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<SubscriptionInfo>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Details_ReturnsViewWithSubscription_WhenSubscriptionExists()
    {
        // Arrange
        var subscription = new SubscriptionInfo { Id = "sub1", Name = "sub1", DisplayName = "Subscription 1" };
        _mockApiManagementService.Setup(s => s.GetSubscriptionAsync("sub1")).ReturnsAsync(subscription);

        // Act
        var result = await _controller.Details("sub1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SubscriptionInfo>(viewResult.Model);
        Assert.Equal("sub1", model.Id);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenSubscriptionDoesNotExist()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.GetSubscriptionAsync("nonexistent")).ReturnsAsync((SubscriptionInfo?)null);

        // Act
        var result = await _controller.Details("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
    {
        // Arrange
        var subscription = new SubscriptionInfo { Id = "sub1", Name = "sub1", DisplayName = "Subscription 1" };
        _mockApiManagementService.Setup(s => s.CreateSubscriptionAsync(It.IsAny<SubscriptionInfo>())).ReturnsAsync(subscription);

        // Act
        var result = await _controller.Create(subscription);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Create_Post_ReturnsViewWithModel_OnException()
    {
        // Arrange
        var subscription = new SubscriptionInfo { Id = "sub1", Name = "sub1", DisplayName = "Subscription 1" };
        _mockApiManagementService.Setup(s => s.CreateSubscriptionAsync(It.IsAny<SubscriptionInfo>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Create(subscription);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(subscription, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task RegenerateKey_RedirectsToDetails()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.RegenerateSubscriptionKeyAsync("sub1", "primary")).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RegenerateKey("sub1", "primary");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal("sub1", redirectResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task RegenerateKey_RedirectsToDetails_OnException()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.RegenerateSubscriptionKeyAsync("sub1", "primary")).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.RegenerateKey("sub1", "primary");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal("sub1", redirectResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task RegenerateKey_ReturnsNotFound_WhenModifyFeatureFlagIsDisabled()
    {
        // Arrange
        var mockFeatureFlagsDisabled = new Mock<IOptions<FeatureFlags>>();
        mockFeatureFlagsDisabled.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = true, EnableModifyOperations = false });
        var controller = new SubscriptionsController(_mockLogger.Object, _mockApiManagementService.Object, mockFeatureFlagsDisabled.Object);

        // Act
        var result = await controller.RegenerateKey("sub1", "primary");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewWithSubscription_WhenSubscriptionExists()
    {
        // Arrange
        var subscription = new SubscriptionInfo { Id = "sub1", Name = "sub1", DisplayName = "Subscription 1" };
        _mockApiManagementService.Setup(s => s.GetSubscriptionAsync("sub1")).ReturnsAsync(subscription);

        // Act
        var result = await _controller.Delete("sub1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SubscriptionInfo>(viewResult.Model);
        Assert.Equal("sub1", model.Id);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFound_WhenSubscriptionDoesNotExist()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.GetSubscriptionAsync("nonexistent")).ReturnsAsync((SubscriptionInfo?)null);

        // Act
        var result = await _controller.Delete("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFound_WhenDeleteFeatureFlagIsDisabled()
    {
        // Arrange
        var mockFeatureFlagsDisabled = new Mock<IOptions<FeatureFlags>>();
        mockFeatureFlagsDisabled.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = false, EnableModifyOperations = true });
        var controller = new SubscriptionsController(_mockLogger.Object, _mockApiManagementService.Object, mockFeatureFlagsDisabled.Object);

        // Act
        var result = await controller.Delete("sub1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex_WhenSubscriptionIsDeleted()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.DeleteSubscriptionAsync("sub1")).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteConfirmed("sub1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex_OnException()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.DeleteSubscriptionAsync("sub1")).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.DeleteConfirmed("sub1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsNotFound_WhenDeleteFeatureFlagIsDisabled()
    {
        // Arrange
        var mockFeatureFlagsDisabled = new Mock<IOptions<FeatureFlags>>();
        mockFeatureFlagsDisabled.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = false, EnableModifyOperations = true });
        var controller = new SubscriptionsController(_mockLogger.Object, _mockApiManagementService.Object, mockFeatureFlagsDisabled.Object);

        // Act
        var result = await controller.DeleteConfirmed("sub1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
