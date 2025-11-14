namespace ApimGuard.Models;

public class AppRegistrationInfo
{
    public string Id { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime? CreatedDateTime { get; set; }
    public List<string> RedirectUris { get; set; } = new();
    public bool HasSecrets { get; set; }
    public bool HasCertificates { get; set; }
    public List<string> Tags { get; set; } = new();
}
