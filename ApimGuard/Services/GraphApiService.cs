using ApimGuard.Models;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Extensions.Options;

namespace ApimGuard.Services;

public class GraphApiService : IGraphApiService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<GraphApiService> _logger;

    public GraphApiService(IOptions<AzureConfiguration> azureConfig, ILogger<GraphApiService> logger)
    {
        _logger = logger;

        var config = azureConfig.Value;
        var credential = new ClientSecretCredential(
            config.TenantId,
            config.ClientId,
            config.ClientSecret
        );

        _graphClient = new GraphServiceClient(credential);
    }

    public async Task<List<AppRegistrationInfo>> GetApplicationsAsync()
    {
        try
        {
            var applications = await _graphClient.Applications.GetAsync();
            var appList = new List<AppRegistrationInfo>();

            if (applications?.Value != null)
            {
                foreach (var app in applications.Value)
                {
                    appList.Add(new AppRegistrationInfo
                    {
                        Id = app.Id ?? string.Empty,
                        AppId = app.AppId ?? string.Empty,
                        DisplayName = app.DisplayName ?? string.Empty,
                        CreatedDateTime = app.CreatedDateTime?.DateTime,
                        HasSecrets = app.PasswordCredentials?.Any() ?? false,
                        HasCertificates = app.KeyCredentials?.Any() ?? false
                    });
                }
            }

            return appList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applications from Microsoft Graph");
            throw;
        }
    }

    public async Task<AppRegistrationInfo?> GetApplicationAsync(string id)
    {
        try
        {
            var app = await _graphClient.Applications[id].GetAsync();

            if (app == null)
                return null;

            var redirectUris = new List<string>();
            if (app.Web?.RedirectUris != null)
            {
                redirectUris.AddRange(app.Web.RedirectUris);
            }
            if (app.PublicClient?.RedirectUris != null)
            {
                redirectUris.AddRange(app.PublicClient.RedirectUris);
            }

            return new AppRegistrationInfo
            {
                Id = app.Id ?? string.Empty,
                AppId = app.AppId ?? string.Empty,
                DisplayName = app.DisplayName ?? string.Empty,
                CreatedDateTime = app.CreatedDateTime?.DateTime,
                RedirectUris = redirectUris,
                HasSecrets = app.PasswordCredentials?.Any() ?? false,
                HasCertificates = app.KeyCredentials?.Any() ?? false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application {Id} from Microsoft Graph", id);
            throw;
        }
    }

    public async Task<AppRegistrationInfo?> GetApplicationByAppIdAsync(string appId)
    {
        try
        {
            // Query applications filtering by appId (client application ID)
            var applications = await _graphClient.Applications
                .GetAsync(config =>
                {
                    config.QueryParameters.Filter = $"appId eq '{appId}'";
                });

            var app = applications?.Value?.FirstOrDefault();
            if (app == null)
                return null;

            var redirectUris = new List<string>();
            if (app.Web?.RedirectUris != null)
            {
                redirectUris.AddRange(app.Web.RedirectUris);
            }
            if (app.PublicClient?.RedirectUris != null)
            {
                redirectUris.AddRange(app.PublicClient.RedirectUris);
            }

            return new AppRegistrationInfo
            {
                Id = app.Id ?? string.Empty,
                AppId = app.AppId ?? string.Empty,
                DisplayName = app.DisplayName ?? string.Empty,
                CreatedDateTime = app.CreatedDateTime?.DateTime,
                RedirectUris = redirectUris,
                HasSecrets = app.PasswordCredentials?.Any() ?? false,
                HasCertificates = app.KeyCredentials?.Any() ?? false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application by AppId {AppId} from Microsoft Graph", appId);
            throw;
        }
    }

    public async Task<AppRegistrationInfo> CreateApplicationAsync(string displayName, List<string>? redirectUris = null)
    {
        try
        {
            var application = new Application
            {
                DisplayName = displayName,
                SignInAudience = "AzureADMyOrg"
            };

            if (redirectUris?.Any() == true)
            {
                application.Web = new Microsoft.Graph.Models.WebApplication
                {
                    RedirectUris = redirectUris
                };
            }

            var createdApp = await _graphClient.Applications.PostAsync(application);

            if (createdApp == null)
                throw new Exception("Failed to create application");

            return new AppRegistrationInfo
            {
                Id = createdApp.Id ?? string.Empty,
                AppId = createdApp.AppId ?? string.Empty,
                DisplayName = createdApp.DisplayName ?? string.Empty,
                CreatedDateTime = createdApp.CreatedDateTime?.DateTime,
                RedirectUris = redirectUris ?? new List<string>(),
                HasSecrets = false,
                HasCertificates = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application {DisplayName} in Microsoft Graph", displayName);
            throw;
        }
    }

    public async Task DeleteApplicationAsync(string id)
    {
        try
        {
            await _graphClient.Applications[id].DeleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application {Id} from Microsoft Graph", id);
            throw;
        }
    }

    public async Task<List<AppSecretInfo>> GetApplicationSecretsAsync(string id)
    {
        try
        {
            var app = await _graphClient.Applications[id].GetAsync();
            var secrets = new List<AppSecretInfo>();

            if (app?.PasswordCredentials != null)
            {
                foreach (var credential in app.PasswordCredentials)
                {
                    secrets.Add(new AppSecretInfo
                    {
                        Id = credential.KeyId?.ToString() ?? string.Empty,
                        DisplayName = credential.DisplayName ?? string.Empty,
                        StartDateTime = credential.StartDateTime?.DateTime,
                        EndDateTime = credential.EndDateTime?.DateTime
                    });
                }
            }

            return secrets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secrets for application {Id} from Microsoft Graph", id);
            throw;
        }
    }

    public async Task<AppSecretInfo> AddApplicationSecretAsync(string id, string displayName, int validityMonths)
    {
        try
        {
            var passwordCredential = new PasswordCredential
            {
                DisplayName = displayName,
                StartDateTime = DateTimeOffset.UtcNow,
                EndDateTime = DateTimeOffset.UtcNow.AddMonths(validityMonths)
            };

            var result = await _graphClient.Applications[id].AddPassword.PostAsync(
                new Microsoft.Graph.Applications.Item.AddPassword.AddPasswordPostRequestBody
                {
                    PasswordCredential = passwordCredential
                });

            if (result == null)
                throw new Exception("Failed to add secret");

            return new AppSecretInfo
            {
                Id = result.KeyId?.ToString() ?? string.Empty,
                DisplayName = result.DisplayName ?? string.Empty,
                SecretValue = result.SecretText,
                StartDateTime = result.StartDateTime?.DateTime,
                EndDateTime = result.EndDateTime?.DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding secret to application {Id} in Microsoft Graph", id);
            throw;
        }
    }

    public async Task<List<AppCertificateInfo>> GetApplicationCertificatesAsync(string id)
    {
        try
        {
            var app = await _graphClient.Applications[id].GetAsync();
            var certificates = new List<AppCertificateInfo>();

            if (app?.KeyCredentials != null)
            {
                foreach (var credential in app.KeyCredentials)
                {
                    certificates.Add(new AppCertificateInfo
                    {
                        Id = credential.KeyId?.ToString() ?? string.Empty,
                        DisplayName = credential.DisplayName ?? string.Empty,
                        Thumbprint = Convert.ToBase64String(credential.CustomKeyIdentifier ?? Array.Empty<byte>()),
                        StartDateTime = credential.StartDateTime?.DateTime,
                        EndDateTime = credential.EndDateTime?.DateTime
                    });
                }
            }

            return certificates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving certificates for application {Id} from Microsoft Graph", id);
            throw;
        }
    }

    public async Task AddApplicationCertificateAsync(string id, byte[] certificateData, string displayName)
    {
        try
        {
            var keyCredential = new KeyCredential
            {
                Type = "AsymmetricX509Cert",
                Usage = "Verify",
                Key = certificateData,
                DisplayName = displayName,
                StartDateTime = DateTimeOffset.UtcNow,
                EndDateTime = DateTimeOffset.UtcNow.AddYears(1)
            };

            await _graphClient.Applications[id].AddKey.PostAsync(
                new Microsoft.Graph.Applications.Item.AddKey.AddKeyPostRequestBody
                {
                    KeyCredential = keyCredential,
                    PasswordCredential = null,
                    Proof = Convert.ToBase64String(certificateData)
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding certificate to application {Id} in Microsoft Graph", id);
            throw;
        }
    }
}
