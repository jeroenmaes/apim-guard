namespace ApimGuard.Models;

public class ApiInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ServiceUrl { get; set; } = string.Empty;
    public List<string> Protocols { get; set; } = new();
    public bool SubscriptionRequired { get; set; }
    public string AzureAdApplicationId { get; set; }
    public List<string> AzureAdClientApplicationIds { get; set; } = new();
}
