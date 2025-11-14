using Microsoft.AspNetCore.Authorization;
using ApimGuard.Controllers;
using Xunit;
using System.Reflection;

namespace ApimGuard.Tests.Controllers;

public class AuthenticationTests
{
    [Fact]
    public void HomeController_ErrorAction_HasAllowAnonymousAttribute()
    {
        // Arrange
        var controllerType = typeof(HomeController);
        var method = controllerType.GetMethod("Error");

        // Act
        var attribute = method?.GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        Assert.NotNull(method);
        Assert.NotNull(attribute);
    }

    [Fact]
    public void HomeController_IndexAction_HasAllowAnonymousAttribute()
    {
        // Arrange
        var controllerType = typeof(HomeController);
        var method = controllerType.GetMethod("Index");

        // Act
        var attribute = method?.GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        Assert.NotNull(method);
        Assert.NotNull(attribute);
    }

    [Fact]
    public void HomeController_PrivacyAction_DoesNotHaveAllowAnonymousAttribute()
    {
        // Arrange
        var controllerType = typeof(HomeController);
        var method = controllerType.GetMethod("Privacy");

        // Act
        var attribute = method?.GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        Assert.NotNull(method);
        Assert.Null(attribute);
    }
}
