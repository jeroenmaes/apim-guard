using ApimGuard.Models;

namespace ApimGuard.Services;

public interface IGraphApiService
{
    Task<List<AppRegistrationInfo>> GetApplicationsAsync();
    Task<AppRegistrationInfo?> GetApplicationAsync(string id);
    Task<AppRegistrationInfo?> GetApplicationByAppIdAsync(string appId);
    Task<AppRegistrationInfo> CreateApplicationAsync(string displayName, List<string>? redirectUris = null);
    Task DeleteApplicationAsync(string id);
    Task<List<AppSecretInfo>> GetApplicationSecretsAsync(string id);
    Task<AppSecretInfo> AddApplicationSecretAsync(string id, string displayName, int validityMonths);
    Task<List<AppCertificateInfo>> GetApplicationCertificatesAsync(string id);
    Task AddApplicationCertificateAsync(string id, byte[] certificateData, string displayName);
}
