using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Models;

public class FeatureFlagsTests
{
    [Fact]
    public void FeatureFlags_DefaultValues_AreBothTrue()
    {
        // Arrange & Act
        var featureFlags = new FeatureFlags();

        // Assert
        Assert.True(featureFlags.EnableDeleteOperations);
        Assert.True(featureFlags.EnableModifyOperations);
    }

    [Fact]
    public void FeatureFlags_CanSetEnableDeleteOperations()
    {
        // Arrange
        var featureFlags = new FeatureFlags { EnableDeleteOperations = false };

        // Act & Assert
        Assert.False(featureFlags.EnableDeleteOperations);
        Assert.True(featureFlags.EnableModifyOperations);
    }

    [Fact]
    public void FeatureFlags_CanSetEnableModifyOperations()
    {
        // Arrange
        var featureFlags = new FeatureFlags { EnableModifyOperations = false };

        // Act & Assert
        Assert.True(featureFlags.EnableDeleteOperations);
        Assert.False(featureFlags.EnableModifyOperations);
    }

    [Fact]
    public void FeatureFlags_CanDisableBothFlags()
    {
        // Arrange
        var featureFlags = new FeatureFlags 
        { 
            EnableDeleteOperations = false,
            EnableModifyOperations = false
        };

        // Act & Assert
        Assert.False(featureFlags.EnableDeleteOperations);
        Assert.False(featureFlags.EnableModifyOperations);
    }
}
