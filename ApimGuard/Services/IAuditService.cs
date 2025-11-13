using ApimGuard.Models;

namespace ApimGuard.Services;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry);
    Task<List<AuditEntry>> GetAuditEntriesAsync();
    Task<List<AuditEntry>> GetAuditEntriesAsync(DateTime fromDate, DateTime toDate);
    Task<List<AuditEntry>> GetAuditEntriesByUserAsync(string userId);
}
