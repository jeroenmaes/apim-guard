namespace ApimGuard.Models;

public class ProductInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string State { get; set; } = string.Empty;
    public bool SubscriptionRequired { get; set; }
    public bool ApprovalRequired { get; set; }
    public int? SubscriptionsLimit { get; set; }
    public string? Terms { get; set; }
}
