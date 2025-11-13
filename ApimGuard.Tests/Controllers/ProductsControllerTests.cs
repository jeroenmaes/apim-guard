using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ApimGuard.Controllers;
using ApimGuard.Models;
using ApimGuard.Services;
using Xunit;

namespace ApimGuard.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly Mock<IApiManagementService> _mockApiManagementService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _mockApiManagementService = new Mock<IApiManagementService>();
        _controller = new ProductsController(_mockLogger.Object, _mockApiManagementService.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithProducts()
    {
        // Arrange
        var products = new List<ProductInfo>
        {
            new ProductInfo { Id = "product1", Name = "product1", DisplayName = "Product 1" },
            new ProductInfo { Id = "product2", Name = "product2", DisplayName = "Product 2" }
        };

        _mockApiManagementService
            .Setup(s => s.GetProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ProductInfo>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task Index_ReturnsEmptyListWhenExceptionOccurs()
    {
        // Arrange
        _mockApiManagementService
            .Setup(s => s.GetProductsAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ProductInfo>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Details_ReturnsViewWithProduct_WhenProductExists()
    {
        // Arrange
        var product = new ProductInfo { Id = "product1", Name = "product1", DisplayName = "Product 1" };

        _mockApiManagementService
            .Setup(s => s.GetProductAsync("product1"))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.Details("product1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductInfo>(viewResult.Model);
        Assert.Equal("product1", model.Id);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        _mockApiManagementService
            .Setup(s => s.GetProductAsync("nonexistent"))
            .ReturnsAsync((ProductInfo?)null);

        // Act
        var result = await _controller.Details("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenExceptionOccurs()
    {
        // Arrange
        _mockApiManagementService
            .Setup(s => s.GetProductAsync("product1"))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Details("product1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_ReturnsView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_RedirectsToIndex_WhenModelStateIsValid()
    {
        // Arrange
        var product = new ProductInfo
        {
            Name = "test-product",
            DisplayName = "Test Product",
            State = "Published"
        };

        _mockApiManagementService
            .Setup(s => s.CreateProductAsync(It.IsAny<ProductInfo>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.Create(product);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Create_Post_ReturnsViewWithModel_WhenExceptionOccurs()
    {
        // Arrange
        var product = new ProductInfo
        {
            Name = "test-product",
            DisplayName = "Test Product"
        };

        _mockApiManagementService
            .Setup(s => s.CreateProductAsync(It.IsAny<ProductInfo>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Create(product);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductInfo>(viewResult.Model);
        Assert.Equal("test-product", model.Name);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Delete_ReturnsViewWithProduct_WhenProductExists()
    {
        // Arrange
        var product = new ProductInfo { Id = "product1", Name = "product1", DisplayName = "Product 1" };

        _mockApiManagementService
            .Setup(s => s.GetProductAsync("product1"))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.Delete("product1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductInfo>(viewResult.Model);
        Assert.Equal("product1", model.Id);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        _mockApiManagementService
            .Setup(s => s.GetProductAsync("nonexistent"))
            .ReturnsAsync((ProductInfo?)null);

        // Act
        var result = await _controller.Delete("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        _mockApiManagementService
            .Setup(s => s.DeleteProductAsync("product1"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteConfirmed("product1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex_WhenExceptionOccurs()
    {
        // Arrange
        _mockApiManagementService
            .Setup(s => s.DeleteProductAsync("product1"))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.DeleteConfirmed("product1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }
}
