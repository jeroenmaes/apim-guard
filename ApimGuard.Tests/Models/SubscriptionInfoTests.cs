using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Models;

public class SubscriptionInfoTests
{
    [Fact]
    public void SubscriptionInfo_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var subscriptionInfo = new SubscriptionInfo();

        // Assert
        Assert.NotNull(subscriptionInfo);
        Assert.Equal(string.Empty, subscriptionInfo.Id);
        Assert.Equal(string.Empty, subscriptionInfo.Name);
        Assert.Equal(string.Empty, subscriptionInfo.DisplayName);
        Assert.Equal(string.Empty, subscriptionInfo.Scope);
        Assert.Equal(string.Empty, subscriptionInfo.State);
    }

    [Fact]
    public void SubscriptionInfo_ShouldSetAndGetProperties()
    {
        // Arrange
        var createdDate = DateTime.UtcNow;
        var expirationDate = DateTime.UtcNow.AddMonths(6);

        var subscriptionInfo = new SubscriptionInfo
        {
            Id = "sub-123",
            Name = "sub-123",
            DisplayName = "Test Subscription",
            Scope = "/apis/test-api",
            State = "active",
            PrimaryKey = "primary-key-value",
            SecondaryKey = "secondary-key-value",
            CreatedDate = createdDate,
            ExpirationDate = expirationDate
        };

        // Assert
        Assert.Equal("sub-123", subscriptionInfo.Id);
        Assert.Equal("sub-123", subscriptionInfo.Name);
        Assert.Equal("Test Subscription", subscriptionInfo.DisplayName);
        Assert.Equal("/apis/test-api", subscriptionInfo.Scope);
        Assert.Equal("active", subscriptionInfo.State);
        Assert.Equal("primary-key-value", subscriptionInfo.PrimaryKey);
        Assert.Equal("secondary-key-value", subscriptionInfo.SecondaryKey);
        Assert.Equal(createdDate, subscriptionInfo.CreatedDate);
        Assert.Equal(expirationDate, subscriptionInfo.ExpirationDate);
    }
}
