using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ApimGuard.Controllers;
using ApimGuard.Models;
using ApimGuard.Services;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ApimGuard.Tests.Controllers;

public class ApiManagementControllerTests
{
    private readonly Mock<ILogger<ApiManagementController>> _mockLogger;
    private readonly Mock<IApiManagementService> _mockApiManagementService;
    private readonly Mock<IOptions<FeatureFlags>> _mockFeatureFlags;
    private readonly ApiManagementController _controller;

    public ApiManagementControllerTests()
    {
        _mockLogger = new Mock<ILogger<ApiManagementController>>();
        _mockApiManagementService = new Mock<IApiManagementService>();
        _mockFeatureFlags = new Mock<IOptions<FeatureFlags>>();
        _mockFeatureFlags.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = true, EnableModifyOperations = true });
        _controller = new ApiManagementController(_mockLogger.Object, _mockApiManagementService.Object, _mockFeatureFlags.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithApis()
    {
        // Arrange
        var apis = new List<ApiInfo>
        {
            new ApiInfo { Id = "api1", Name = "api1", DisplayName = "API 1" },
            new ApiInfo { Id = "api2", Name = "api2", DisplayName = "API 2" }
        };
        _mockApiManagementService.Setup(s => s.GetApisAsync()).ReturnsAsync(apis);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ApiInfo>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task Index_ReturnsEmptyListOnException()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.GetApisAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ApiInfo>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Details_ReturnsViewWithApi_WhenApiExists()
    {
        // Arrange
        var api = new ApiInfo { Id = "api1", Name = "api1", DisplayName = "API 1" };
        _mockApiManagementService.Setup(s => s.GetApiAsync("api1")).ReturnsAsync(api);

        // Act
        var result = await _controller.Details("api1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApiInfo>(viewResult.Model);
        Assert.Equal("api1", model.Id);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenApiDoesNotExist()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.GetApiAsync("nonexistent")).ReturnsAsync((ApiInfo?)null);

        // Act
        var result = await _controller.Details("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_OnException()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.GetApiAsync("api1")).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Details("api1");

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
        var api = new ApiInfo { Id = "api1", Name = "api1", DisplayName = "API 1" };
        _mockApiManagementService.Setup(s => s.CreateApiAsync(It.IsAny<ApiInfo>())).ReturnsAsync(api);

        // Act
        var result = await _controller.Create(api);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Create_Post_ReturnsViewWithModel_OnException()
    {
        // Arrange
        var api = new ApiInfo { Id = "api1", Name = "api1", DisplayName = "API 1" };
        _mockApiManagementService.Setup(s => s.CreateApiAsync(It.IsAny<ApiInfo>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Create(api);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(api, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_Post_WithSpecificationFile_CreatesApiFromSpecification()
    {
        // Arrange
        var specContent = "{\"openapi\":\"3.0.0\"}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(specContent);
        var stream = new MemoryStream(bytes);
        var formFile = new FormFile(stream, 0, bytes.Length, "spec", "openapi.json")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/json"
        };

        var api = new ApiInfo
        {
            Id = "api1",
            Name = "api1",
            DisplayName = "API 1",
            Path = "/api/v1",
            SpecificationFile = formFile
        };

        _mockApiManagementService.Setup(s => s.CreateApiFromSpecificationAsync(
            It.IsAny<ApiInfo>(),
            It.IsAny<Stream>(),
            It.IsAny<string>()
        )).ReturnsAsync(api);

        // Act
        var result = await _controller.Create(api);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockApiManagementService.Verify(s => s.CreateApiFromSpecificationAsync(
            It.IsAny<ApiInfo>(),
            It.IsAny<Stream>(),
            "openapi+json"
        ), Times.Once);
    }

    [Fact]
    public async Task Create_Post_WithoutSpecificationFile_CreatesApiNormally()
    {
        // Arrange
        var api = new ApiInfo
        {
            Id = "api1",
            Name = "api1",
            DisplayName = "API 1",
            Path = "/api/v1",
            ServiceUrl = "https://backend.example.com",
            SpecificationFile = null
        };

        _mockApiManagementService.Setup(s => s.CreateApiAsync(It.IsAny<ApiInfo>())).ReturnsAsync(api);

        // Act
        var result = await _controller.Create(api);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockApiManagementService.Verify(s => s.CreateApiAsync(It.IsAny<ApiInfo>()), Times.Once);
        _mockApiManagementService.Verify(s => s.CreateApiFromSpecificationAsync(
            It.IsAny<ApiInfo>(),
            It.IsAny<Stream>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task Create_Post_WithYamlSpecificationFile_UsesCorrectFormat()
    {
        // Arrange
        var specContent = "openapi: 3.0.0";
        var bytes = System.Text.Encoding.UTF8.GetBytes(specContent);
        var stream = new MemoryStream(bytes);
        var formFile = new FormFile(stream, 0, bytes.Length, "spec", "openapi.yaml")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/x-yaml"
        };

        var api = new ApiInfo
        {
            Id = "api1",
            Name = "api1",
            DisplayName = "API 1",
            Path = "/api/v1",
            SpecificationFile = formFile
        };

        _mockApiManagementService.Setup(s => s.CreateApiFromSpecificationAsync(
            It.IsAny<ApiInfo>(),
            It.IsAny<Stream>(),
            It.IsAny<string>()
        )).ReturnsAsync(api);

        // Act
        var result = await _controller.Create(api);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockApiManagementService.Verify(s => s.CreateApiFromSpecificationAsync(
            It.IsAny<ApiInfo>(),
            It.IsAny<Stream>(),
            "openapi"
        ), Times.Once);
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewWithApi_WhenApiExists()
    {
        // Arrange
        var api = new ApiInfo { Id = "api1", Name = "api1", DisplayName = "API 1" };
        _mockApiManagementService.Setup(s => s.GetApiAsync("api1")).ReturnsAsync(api);

        // Act
        var result = await _controller.Delete("api1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApiInfo>(viewResult.Model);
        Assert.Equal("api1", model.Id);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFound_WhenApiDoesNotExist()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.GetApiAsync("nonexistent")).ReturnsAsync((ApiInfo?)null);

        // Act
        var result = await _controller.Delete("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.DeleteApiAsync("api1")).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteConfirmed("api1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex_OnException()
    {
        // Arrange
        _mockApiManagementService.Setup(s => s.DeleteApiAsync("api1")).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.DeleteConfirmed("api1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFound_WhenDeleteFeatureFlagIsDisabled()
    {
        // Arrange
        var mockFeatureFlagsDisabled = new Mock<IOptions<FeatureFlags>>();
        mockFeatureFlagsDisabled.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = false, EnableModifyOperations = true });
        var controller = new ApiManagementController(_mockLogger.Object, _mockApiManagementService.Object, mockFeatureFlagsDisabled.Object);

        // Act
        var result = await controller.Delete("api1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsNotFound_WhenDeleteFeatureFlagIsDisabled()
    {
        // Arrange
        var mockFeatureFlagsDisabled = new Mock<IOptions<FeatureFlags>>();
        mockFeatureFlagsDisabled.Setup(f => f.Value).Returns(new FeatureFlags { EnableDeleteOperations = false, EnableModifyOperations = true });
        var controller = new ApiManagementController(_mockLogger.Object, _mockApiManagementService.Object, mockFeatureFlagsDisabled.Object);

        // Act
        var result = await controller.DeleteConfirmed("api1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
