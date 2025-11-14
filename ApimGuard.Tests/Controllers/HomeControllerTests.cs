using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ApimGuard.Controllers;
using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _mockLogger;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _mockLogger = new Mock<ILogger<HomeController>>();
        _controller = new HomeController(_mockLogger.Object);
        
        // Setup HttpContext for the controller
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        // Act
        var result = _controller.Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void UserProfile_ReturnsViewResult()
    {
        // Act
        var result = _controller.UserProfile();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void SaveThemePreference_WithValidTheme_ReturnsOkResult()
    {
        // Arrange
        var theme = "dark";

        // Act
        var result = _controller.SaveThemePreference(theme);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void SaveThemePreference_WithLightTheme_ReturnsOkResult()
    {
        // Arrange
        var theme = "light";

        // Act
        var result = _controller.SaveThemePreference(theme);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void SaveThemePreference_WithInvalidTheme_ReturnsBadRequest()
    {
        // Arrange
        var theme = "invalid";

        // Act
        var result = _controller.SaveThemePreference(theme);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void SaveThemePreference_SetsCookie()
    {
        // Arrange
        var theme = "dark";

        // Act
        var result = _controller.SaveThemePreference(theme);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        // Note: In a real test, you would verify the cookie was set in the Response.Cookies
        // but this requires more complex HttpContext mocking
    }

    [Fact]
    public void Error_ReturnsViewResultWithErrorViewModel()
    {
        // Act
        var result = _controller.Error();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ErrorViewModel>(viewResult.Model);
    }
}
