using ApimGuard.Models;
using ApimGuard.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApimGuard.Tests.Services;

public class AuditServiceTests
{
    private readonly Mock<ILogger<AuditService>> _mockLogger;
    private readonly AuditService _auditService;

    public AuditServiceTests()
    {
        _mockLogger = new Mock<ILogger<AuditService>>();
        _auditService = new AuditService(_mockLogger.Object);
    }

    [Fact]
    public async Task LogAsync_AddsEntryToCollection()
    {
        // Arrange
        var entry = new AuditEntry
        {
            Action = "TestAction",
            Controller = "TestController",
            Method = "GET",
            Path = "/test"
        };

        // Act
        await _auditService.LogAsync(entry);
        var entries = await _auditService.GetAuditEntriesAsync();

        // Assert
        Assert.Single(entries);
        Assert.Equal("TestAction", entries[0].Action);
    }

    [Fact]
    public async Task LogAsync_ThrowsException_WhenEntryIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _auditService.LogAsync(null!));
    }

    [Fact]
    public async Task GetAuditEntriesAsync_ReturnsEntriesOrderedByTimestamp()
    {
        // Arrange
        var entry1 = new AuditEntry { Action = "Action1", Timestamp = DateTime.UtcNow.AddMinutes(-2) };
        var entry2 = new AuditEntry { Action = "Action2", Timestamp = DateTime.UtcNow.AddMinutes(-1) };
        var entry3 = new AuditEntry { Action = "Action3", Timestamp = DateTime.UtcNow };

        await _auditService.LogAsync(entry1);
        await _auditService.LogAsync(entry2);
        await _auditService.LogAsync(entry3);

        // Act
        var entries = await _auditService.GetAuditEntriesAsync();

        // Assert
        Assert.Equal(3, entries.Count);
        Assert.Equal("Action3", entries[0].Action);
        Assert.Equal("Action2", entries[1].Action);
        Assert.Equal("Action1", entries[2].Action);
    }

    [Fact]
    public async Task GetAuditEntriesAsync_WithDateRange_ReturnsFilteredEntries()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var entry1 = new AuditEntry { Action = "Action1", Timestamp = baseTime.AddHours(-3) };
        var entry2 = new AuditEntry { Action = "Action2", Timestamp = baseTime.AddHours(-1) };
        var entry3 = new AuditEntry { Action = "Action3", Timestamp = baseTime };

        await _auditService.LogAsync(entry1);
        await _auditService.LogAsync(entry2);
        await _auditService.LogAsync(entry3);

        // Act
        var entries = await _auditService.GetAuditEntriesAsync(baseTime.AddHours(-2), baseTime.AddHours(1));

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.Action == "Action2");
        Assert.Contains(entries, e => e.Action == "Action3");
    }

    [Fact]
    public async Task GetAuditEntriesByUserAsync_ReturnsUserSpecificEntries()
    {
        // Arrange
        var entry1 = new AuditEntry { Action = "Action1", UserId = "user1" };
        var entry2 = new AuditEntry { Action = "Action2", UserId = "user2" };
        var entry3 = new AuditEntry { Action = "Action3", UserId = "user1" };

        await _auditService.LogAsync(entry1);
        await _auditService.LogAsync(entry2);
        await _auditService.LogAsync(entry3);

        // Act
        var entries = await _auditService.GetAuditEntriesByUserAsync("user1");

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.All(entries, e => Assert.Equal("user1", e.UserId));
    }

    [Fact]
    public async Task GetAuditEntriesByUserAsync_ThrowsException_WhenUserIdIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _auditService.GetAuditEntriesByUserAsync(null!));
    }

    [Fact]
    public async Task GetAuditEntriesByUserAsync_ThrowsException_WhenUserIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _auditService.GetAuditEntriesByUserAsync(string.Empty));
    }

    [Fact]
    public async Task GetAuditEntriesByUserAsync_ReturnsEmptyList_WhenNoMatchingUser()
    {
        // Arrange
        var entry = new AuditEntry { Action = "Action1", UserId = "user1" };
        await _auditService.LogAsync(entry);

        // Act
        var entries = await _auditService.GetAuditEntriesByUserAsync("user2");

        // Assert
        Assert.Empty(entries);
    }

    [Fact]
    public async Task GetAuditEntriesAsync_ReturnsEmptyList_WhenNoEntries()
    {
        // Act
        var entries = await _auditService.GetAuditEntriesAsync();

        // Assert
        Assert.Empty(entries);
    }
}
