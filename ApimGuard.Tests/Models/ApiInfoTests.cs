using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Models;

public class ApiInfoTests
{
    [Fact]
    public void ApiInfo_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var apiInfo = new ApiInfo();

        // Assert
        Assert.NotNull(apiInfo);
        Assert.Equal(string.Empty, apiInfo.Id);
        Assert.Equal(string.Empty, apiInfo.Name);
        Assert.Equal(string.Empty, apiInfo.DisplayName);
        Assert.Equal(string.Empty, apiInfo.Path);
        Assert.Null(apiInfo.Description);
        Assert.Equal(string.Empty, apiInfo.ServiceUrl);
        Assert.NotNull(apiInfo.Protocols);
        Assert.Empty(apiInfo.Protocols);
        Assert.False(apiInfo.SubscriptionRequired);
        Assert.NotNull(apiInfo.AzureAdApplicationIds);
        Assert.Empty(apiInfo.AzureAdApplicationIds);
    }

    [Fact]
    public void ApiInfo_ShouldSetAndGetProperties()
    {
        // Arrange
        var apiInfo = new ApiInfo
        {
            Id = "test-api",
            Name = "test-api",
            DisplayName = "Test API",
            Path = "/test",
            Description = "A test API",
            ServiceUrl = "https://api.test.com",
            Protocols = new List<string> { "https", "http" }
        };

        // Assert
        Assert.Equal("test-api", apiInfo.Id);
        Assert.Equal("test-api", apiInfo.Name);
        Assert.Equal("Test API", apiInfo.DisplayName);
        Assert.Equal("/test", apiInfo.Path);
        Assert.Equal("A test API", apiInfo.Description);
        Assert.Equal("https://api.test.com", apiInfo.ServiceUrl);
        Assert.Equal(2, apiInfo.Protocols.Count);
        Assert.Contains("https", apiInfo.Protocols);
        Assert.Contains("http", apiInfo.Protocols);
    }

    [Fact]
    public void ApiInfo_ShouldSetAndGetSecurityProperties()
    {
        // Arrange
        var apiInfo = new ApiInfo
        {
            Id = "test-api",
            Name = "test-api",
            DisplayName = "Test API",
            Path = "/test",
            SubscriptionRequired = true,
            AzureAdApplicationIds = new List<string> { "app-id-1", "app-id-2" }
        };

        // Assert
        Assert.True(apiInfo.SubscriptionRequired);
        Assert.Equal(2, apiInfo.AzureAdApplicationIds.Count);
        Assert.Contains("app-id-1", apiInfo.AzureAdApplicationIds);
        Assert.Contains("app-id-2", apiInfo.AzureAdApplicationIds);
    }
}
