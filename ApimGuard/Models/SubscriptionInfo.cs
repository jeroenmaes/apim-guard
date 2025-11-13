namespace ApimGuard.Models;

public class SubscriptionInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? PrimaryKey { get; set; }
    public string? SecondaryKey { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
