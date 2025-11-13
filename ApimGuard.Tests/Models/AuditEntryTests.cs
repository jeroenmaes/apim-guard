using ApimGuard.Models;
using Xunit;

namespace ApimGuard.Tests.Models;

public class AuditEntryTests
{
    [Fact]
    public void AuditEntry_HasDefaultValues()
    {
        // Act
        var entry = new AuditEntry();

        // Assert
        Assert.NotNull(entry.Id);
        Assert.NotEqual(default(DateTime), entry.Timestamp);
        Assert.NotNull(entry.AdditionalData);
        Assert.Empty(entry.AdditionalData);
    }

    [Fact]
    public void AuditEntry_CanSetProperties()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var additionalData = new Dictionary<string, string> { { "key", "value" } };

        // Act
        var entry = new AuditEntry
        {
            Id = "test-id",
            Timestamp = timestamp,
            UserId = "user123",
            UserName = "testuser",
            Action = "TestAction",
            Controller = "TestController",
            Method = "POST",
            Path = "/test/path",
            StatusCode = 200,
            IpAddress = "127.0.0.1",
            AdditionalData = additionalData
        };

        // Assert
        Assert.Equal("test-id", entry.Id);
        Assert.Equal(timestamp, entry.Timestamp);
        Assert.Equal("user123", entry.UserId);
        Assert.Equal("testuser", entry.UserName);
        Assert.Equal("TestAction", entry.Action);
        Assert.Equal("TestController", entry.Controller);
        Assert.Equal("POST", entry.Method);
        Assert.Equal("/test/path", entry.Path);
        Assert.Equal(200, entry.StatusCode);
        Assert.Equal("127.0.0.1", entry.IpAddress);
        Assert.Equal(additionalData, entry.AdditionalData);
    }

    [Fact]
    public void AuditEntry_GeneratesUniqueIds()
    {
        // Act
        var entry1 = new AuditEntry();
        var entry2 = new AuditEntry();

        // Assert
        Assert.NotEqual(entry1.Id, entry2.Id);
    }

    [Fact]
    public void AuditEntry_SetsTimestampToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entry = new AuditEntry();

        // Assert
        var afterCreation = DateTime.UtcNow;
        Assert.InRange(entry.Timestamp, beforeCreation.AddSeconds(-1), afterCreation.AddSeconds(1));
    }
}
