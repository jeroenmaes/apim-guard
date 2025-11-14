using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Models;

public class AppRegistrationInfoTests
{
    [Fact]
    public void AppRegistrationInfo_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var appInfo = new AppRegistrationInfo();

        // Assert
        Assert.NotNull(appInfo);
        Assert.Equal(string.Empty, appInfo.Id);
        Assert.Equal(string.Empty, appInfo.AppId);
        Assert.Equal(string.Empty, appInfo.DisplayName);
        Assert.Null(appInfo.CreatedDateTime);
        Assert.NotNull(appInfo.RedirectUris);
        Assert.Empty(appInfo.RedirectUris);
        Assert.False(appInfo.HasSecrets);
        Assert.False(appInfo.HasCertificates);
        Assert.NotNull(appInfo.Tags);
        Assert.Empty(appInfo.Tags);
        Assert.Equal(AppRegistrationType.None, appInfo.Type);
        Assert.Null(appInfo.ApiPermissionClientId);
        Assert.Null(appInfo.ApiPermissionAppRole);
    }

    [Fact]
    public void AppRegistrationInfo_ShouldSetAndGetProperties()
    {
        // Arrange
        var createdDate = DateTime.UtcNow;
        var appInfo = new AppRegistrationInfo
        {
            Id = "test-id",
            AppId = "test-app-id",
            DisplayName = "Test App",
            CreatedDateTime = createdDate,
            RedirectUris = new List<string> { "https://localhost", "https://app.test.com" },
            HasSecrets = true,
            HasCertificates = false,
            Tags = new List<string> { "APIMSEC", "Production" }
        };

        // Assert
        Assert.Equal("test-id", appInfo.Id);
        Assert.Equal("test-app-id", appInfo.AppId);
        Assert.Equal("Test App", appInfo.DisplayName);
        Assert.Equal(createdDate, appInfo.CreatedDateTime);
        Assert.Equal(2, appInfo.RedirectUris.Count);
        Assert.Contains("https://localhost", appInfo.RedirectUris);
        Assert.Contains("https://app.test.com", appInfo.RedirectUris);
        Assert.True(appInfo.HasSecrets);
        Assert.False(appInfo.HasCertificates);
        Assert.Equal(2, appInfo.Tags.Count);
        Assert.Contains("APIMSEC", appInfo.Tags);
        Assert.Contains("Production", appInfo.Tags);
    }

    [Fact]
    public void AppRegistrationInfo_Tags_ShouldBeEmptyListByDefault()
    {
        // Arrange & Act
        var appInfo = new AppRegistrationInfo();

        // Assert
        Assert.NotNull(appInfo.Tags);
        Assert.Empty(appInfo.Tags);
        Assert.IsType<List<string>>(appInfo.Tags);
    }

    [Fact]
    public void AppRegistrationInfo_Tags_ShouldAllowMultipleValues()
    {
        // Arrange
        var appInfo = new AppRegistrationInfo
        {
            Tags = new List<string> { "APIMSEC", "Development", "TestTag" }
        };

        // Assert
        Assert.Equal(3, appInfo.Tags.Count);
        Assert.Contains("APIMSEC", appInfo.Tags);
        Assert.Contains("Development", appInfo.Tags);
        Assert.Contains("TestTag", appInfo.Tags);
    }
}
