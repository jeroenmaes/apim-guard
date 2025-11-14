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
                    // Filter: only include apps with the 'APIMSEC' tag
                    if (app.Tags != null && app.Tags.Contains("APIMSEC"))
                    {
                        appList.Add(new AppRegistrationInfo
                        {
                            Id = app.Id ?? string.Empty,
                            AppId = app.AppId ?? string.Empty,
                            DisplayName = app.DisplayName ?? string.Empty,
                            CreatedDateTime = app.CreatedDateTime?.DateTime,
                            HasSecrets = app.PasswordCredentials?.Any() ?? false,
                            HasCertificates = app.KeyCredentials?.Any() ?? false,
                            Tags = app.Tags?.ToList() ?? new List<string>()
                        });
                    }
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
                HasCertificates = app.KeyCredentials?.Any() ?? false,
                Tags = app.Tags?.ToList() ?? new List<string>()
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
                HasCertificates = app.KeyCredentials?.Any() ?? false,
                Tags = app.Tags?.ToList() ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application by AppId {AppId} from Microsoft Graph", appId);
            throw;
        }
    }

    public async Task<AppRegistrationInfo> CreateApplicationAsync(AppRegistrationInfo appInfo)
    {
        try
        {
            var application = new Application
            {
                DisplayName = appInfo.DisplayName,
                SignInAudience = "AzureADMyOrg",
                Tags = new List<string> { "APIMSEC" }
            };

            if (appInfo.RedirectUris?.Any() == true)
            {
                application.Web = new Microsoft.Graph.Models.WebApplication
                {
                    RedirectUris = appInfo.RedirectUris
                };
            }

            // Backend API specific configuration
            if (appInfo.Type == AppRegistrationType.BackendApi)
            {
                // Set application ID URI
                var appIdUri = $"api://{Guid.NewGuid()}";
                application.IdentifierUris = new List<string> { appIdUri };

                // Create the API.Access app role
                application.AppRoles = new List<AppRole>
                {
                    new AppRole
                    {
                        Id = Guid.NewGuid(),
                        AllowedMemberTypes = new List<string> { "User", "Application" },
                        Description = "API.Access",
                        DisplayName = "API.Access",
                        IsEnabled = true,
                        Value = "API.Access"
                    }
                };
            }

            var createdApp = await _graphClient.Applications.PostAsync(application);

            if (createdApp == null)
                throw new Exception("Failed to create application");

            // Backend API: Set "Assignment required" on the service principal
            if (appInfo.Type == AppRegistrationType.BackendApi)
            {
                // Create service principal for the application
                var servicePrincipal = new ServicePrincipal
                {
                    AppId = createdApp.AppId,
                    AppRoleAssignmentRequired = true
                };

                await _graphClient.ServicePrincipals.PostAsync(servicePrincipal);
            }

            // Consumer API: Add API Permission
            if (appInfo.Type == AppRegistrationType.ConsumerApi && 
                !string.IsNullOrEmpty(appInfo.ApiPermissionClientId) && 
                !string.IsNullOrEmpty(appInfo.ApiPermissionAppRole))
            {
                // Find the service principal for the resource API
                var resourceApp = await _graphClient.Applications
                    .GetAsync(config =>
                    {
                        config.QueryParameters.Filter = $"appId eq '{appInfo.ApiPermissionClientId}'";
                    });

                var resourceAppObject = resourceApp?.Value?.FirstOrDefault();
                if (resourceAppObject != null)
                {
                    // Find the service principal
                    var resourceSps = await _graphClient.ServicePrincipals
                        .GetAsync(config =>
                        {
                            config.QueryParameters.Filter = $"appId eq '{appInfo.ApiPermissionClientId}'";
                        });
                    
                    var resourceSp = resourceSps?.Value?.FirstOrDefault();
                    
                    if (resourceSp != null && resourceAppObject.AppRoles != null)
                    {
                        // Find the specific app role
                        var appRole = resourceAppObject.AppRoles.FirstOrDefault(r => r.Value == appInfo.ApiPermissionAppRole);
                        
                        if (appRole != null && appRole.Id.HasValue)
                        {
                            // Add the required resource access
                            var requiredResourceAccess = new RequiredResourceAccess
                            {
                                ResourceAppId = appInfo.ApiPermissionClientId,
                                ResourceAccess = new List<ResourceAccess>
                                {
                                    new ResourceAccess
                                    {
                                        Id = appRole.Id.Value,
                                        Type = "Role"
                                    }
                                }
                            };

                            // Update the application with the required resource access
                            var updateApp = new Application
                            {
                                RequiredResourceAccess = new List<RequiredResourceAccess> { requiredResourceAccess }
                            };

                            await _graphClient.Applications[createdApp.Id].PatchAsync(updateApp);
                        }
                    }
                }
            }

            return new AppRegistrationInfo
            {
                Id = createdApp.Id ?? string.Empty,
                AppId = createdApp.AppId ?? string.Empty,
                DisplayName = createdApp.DisplayName ?? string.Empty,
                CreatedDateTime = createdApp.CreatedDateTime?.DateTime,
                RedirectUris = appInfo.RedirectUris ?? new List<string>(),
                HasSecrets = false,
                HasCertificates = false,
                Tags = createdApp.Tags?.ToList() ?? new List<string> { "APIMSEC" },
                Type = appInfo.Type
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application {DisplayName} in Microsoft Graph", appInfo.DisplayName);
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
