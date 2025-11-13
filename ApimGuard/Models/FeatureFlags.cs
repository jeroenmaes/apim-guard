namespace ApimGuard.Models;

public class FeatureFlags
{
    public bool EnableDeleteOperations { get; set; } = true;
    public bool EnableModifyOperations { get; set; } = true;
}
