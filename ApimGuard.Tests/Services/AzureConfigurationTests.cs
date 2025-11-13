using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Services;

public class AzureConfigurationTests
{
    [Fact]
    public void AzureConfiguration_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var config = new AzureConfiguration();

        // Assert
        Assert.NotNull(config);
        Assert.Equal(string.Empty, config.TenantId);
        Assert.Equal(string.Empty, config.SubscriptionId);
        Assert.Equal(string.Empty, config.ResourceGroupName);
        Assert.Equal(string.Empty, config.ApiManagementServiceName);
        Assert.Equal(string.Empty, config.ClientId);
        Assert.Equal(string.Empty, config.ClientSecret);
    }

    [Fact]
    public void AzureConfiguration_ShouldSetAndGetProperties()
    {
        // Arrange & Act
        var config = new AzureConfiguration
        {
            TenantId = "tenant-123",
            SubscriptionId = "sub-123",
            ResourceGroupName = "rg-test",
            ApiManagementServiceName = "apim-test",
            ClientId = "client-123",
            ClientSecret = "secret-123"
        };

        // Assert
        Assert.Equal("tenant-123", config.TenantId);
        Assert.Equal("sub-123", config.SubscriptionId);
        Assert.Equal("rg-test", config.ResourceGroupName);
        Assert.Equal("apim-test", config.ApiManagementServiceName);
        Assert.Equal("client-123", config.ClientId);
        Assert.Equal("secret-123", config.ClientSecret);
    }
}
