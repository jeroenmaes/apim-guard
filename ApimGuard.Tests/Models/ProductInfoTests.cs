using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Models;

public class ProductInfoTests
{
    [Fact]
    public void ProductInfo_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var productInfo = new ProductInfo();

        // Assert
        Assert.NotNull(productInfo);
        Assert.Equal(string.Empty, productInfo.Id);
        Assert.Equal(string.Empty, productInfo.Name);
        Assert.Equal(string.Empty, productInfo.DisplayName);
        Assert.Null(productInfo.Description);
        Assert.Equal(string.Empty, productInfo.State);
        Assert.False(productInfo.SubscriptionRequired);
        Assert.False(productInfo.ApprovalRequired);
        Assert.Null(productInfo.SubscriptionsLimit);
        Assert.Null(productInfo.Terms);
    }

    [Fact]
    public void ProductInfo_ShouldSetAndGetProperties()
    {
        // Arrange
        var productInfo = new ProductInfo
        {
            Id = "test-product",
            Name = "test-product",
            DisplayName = "Test Product",
            Description = "A test product",
            State = "Published",
            SubscriptionRequired = true,
            ApprovalRequired = false,
            SubscriptionsLimit = 10,
            Terms = "Terms and conditions apply"
        };

        // Assert
        Assert.Equal("test-product", productInfo.Id);
        Assert.Equal("test-product", productInfo.Name);
        Assert.Equal("Test Product", productInfo.DisplayName);
        Assert.Equal("A test product", productInfo.Description);
        Assert.Equal("Published", productInfo.State);
        Assert.True(productInfo.SubscriptionRequired);
        Assert.False(productInfo.ApprovalRequired);
        Assert.Equal(10, productInfo.SubscriptionsLimit);
        Assert.Equal("Terms and conditions apply", productInfo.Terms);
    }

    [Fact]
    public void ProductInfo_ShouldHandleNullableProperties()
    {
        // Arrange
        var productInfo = new ProductInfo
        {
            Id = "test-product",
            Name = "test-product",
            DisplayName = "Test Product",
            Description = null,
            SubscriptionsLimit = null,
            Terms = null
        };

        // Assert
        Assert.Null(productInfo.Description);
        Assert.Null(productInfo.SubscriptionsLimit);
        Assert.Null(productInfo.Terms);
    }

    [Fact]
    public void ProductInfo_ShouldSetStateToPublished()
    {
        // Arrange
        var productInfo = new ProductInfo
        {
            Id = "test-product",
            Name = "test-product",
            DisplayName = "Test Product",
            State = "Published"
        };

        // Assert
        Assert.Equal("Published", productInfo.State);
    }

    [Fact]
    public void ProductInfo_ShouldSetStateToNotPublished()
    {
        // Arrange
        var productInfo = new ProductInfo
        {
            Id = "test-product",
            Name = "test-product",
            DisplayName = "Test Product",
            State = "NotPublished"
        };

        // Assert
        Assert.Equal("NotPublished", productInfo.State);
    }

    [Fact]
    public void ProductInfo_ShouldSetApprovalAndSubscriptionRequirements()
    {
        // Arrange
        var productInfo = new ProductInfo
        {
            Id = "test-product",
            Name = "test-product",
            DisplayName = "Test Product",
            SubscriptionRequired = true,
            ApprovalRequired = true
        };

        // Assert
        Assert.True(productInfo.SubscriptionRequired);
        Assert.True(productInfo.ApprovalRequired);
    }
}
