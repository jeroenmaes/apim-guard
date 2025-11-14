namespace ApimGuard.Models;

public class RequiredClaim
{
    public string Name { get; set; } = string.Empty;
    public string Match { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
}
