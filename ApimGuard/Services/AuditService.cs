using ApimGuard.Models;
using System.Collections.Concurrent;

namespace ApimGuard.Services;

public class AuditService : IAuditService
{
    private readonly ConcurrentBag<AuditEntry> _auditEntries = new();
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(AuditEntry entry)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        _auditEntries.Add(entry);
        _logger.LogInformation("Audit entry logged: {Action} by {User} at {Timestamp}", 
            entry.Action, entry.UserName ?? "Anonymous", entry.Timestamp);
        
        return Task.CompletedTask;
    }

    public Task<List<AuditEntry>> GetAuditEntriesAsync()
    {
        return Task.FromResult(_auditEntries.OrderByDescending(e => e.Timestamp).ToList());
    }

    public Task<List<AuditEntry>> GetAuditEntriesAsync(DateTime fromDate, DateTime toDate)
    {
        var filtered = _auditEntries
            .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
        
        return Task.FromResult(filtered);
    }

    public Task<List<AuditEntry>> GetAuditEntriesByUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        var filtered = _auditEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
        
        return Task.FromResult(filtered);
    }
}
