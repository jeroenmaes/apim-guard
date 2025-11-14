using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Models;

public class AppRegistrationTypeTests
{
    [Fact]
    public void AppRegistrationType_ShouldHaveNoneValue()
    {
        // Assert
        Assert.Equal(0, (int)AppRegistrationType.None);
    }

    [Fact]
    public void AppRegistrationType_ShouldHaveBackendApiValue()
    {
        // Assert
        Assert.Equal(1, (int)AppRegistrationType.BackendApi);
    }

    [Fact]
    public void AppRegistrationType_ShouldHaveConsumerApiValue()
    {
        // Assert
        Assert.Equal(2, (int)AppRegistrationType.ConsumerApi);
    }

    [Fact]
    public void AppRegistrationInfo_ShouldDefaultToNoneType()
    {
        // Arrange & Act
        var appInfo = new AppRegistrationInfo();

        // Assert
        Assert.Equal(AppRegistrationType.None, appInfo.Type);
    }

    [Fact]
    public void AppRegistrationInfo_ShouldAllowSettingTypeToBackendApi()
    {
        // Arrange & Act
        var appInfo = new AppRegistrationInfo
        {
            Type = AppRegistrationType.BackendApi
        };

        // Assert
        Assert.Equal(AppRegistrationType.BackendApi, appInfo.Type);
    }

    [Fact]
    public void AppRegistrationInfo_ShouldAllowSettingTypeToConsumerApi()
    {
        // Arrange & Act
        var appInfo = new AppRegistrationInfo
        {
            Type = AppRegistrationType.ConsumerApi
        };

        // Assert
        Assert.Equal(AppRegistrationType.ConsumerApi, appInfo.Type);
    }

    [Fact]
    public void AppRegistrationInfo_ShouldAllowSettingApiPermissionClientId()
    {
        // Arrange
        var clientId = "12345678-1234-1234-1234-123456789012";
        var appInfo = new AppRegistrationInfo
        {
            ApiPermissionClientId = clientId
        };

        // Assert
        Assert.Equal(clientId, appInfo.ApiPermissionClientId);
    }

    [Fact]
    public void AppRegistrationInfo_ShouldAllowSettingApiPermissionAppRole()
    {
        // Arrange
        var appRole = "API.Access";
        var appInfo = new AppRegistrationInfo
        {
            ApiPermissionAppRole = appRole
        };

        // Assert
        Assert.Equal(appRole, appInfo.ApiPermissionAppRole);
    }

    [Fact]
    public void AppRegistrationInfo_ApiPermissionFields_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var appInfo = new AppRegistrationInfo();

        // Assert
        Assert.Null(appInfo.ApiPermissionClientId);
        Assert.Null(appInfo.ApiPermissionAppRole);
    }

    [Fact]
    public void AppRegistrationInfo_ConsumerApiType_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var clientId = "12345678-1234-1234-1234-123456789012";
        var appRole = "API.Access";
        
        var appInfo = new AppRegistrationInfo
        {
            DisplayName = "Consumer App",
            Type = AppRegistrationType.ConsumerApi,
            ApiPermissionClientId = clientId,
            ApiPermissionAppRole = appRole
        };

        // Assert
        Assert.Equal("Consumer App", appInfo.DisplayName);
        Assert.Equal(AppRegistrationType.ConsumerApi, appInfo.Type);
        Assert.Equal(clientId, appInfo.ApiPermissionClientId);
        Assert.Equal(appRole, appInfo.ApiPermissionAppRole);
    }
}
