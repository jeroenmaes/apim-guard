using ApimGuard.Models;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager.ApiManagement.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace ApimGuard.Services;

public class ApiManagementService : IApiManagementService
{
    private readonly ArmClient _armClient;
    private readonly AzureConfiguration _config;
    private readonly ILogger<ApiManagementService> _logger;
    private ApiManagementServiceResource? _apimService;

    public ApiManagementService(IOptions<AzureConfiguration> azureConfig, ILogger<ApiManagementService> logger)
    {
        _logger = logger;
        _config = azureConfig.Value;

        var credential = new ClientSecretCredential(
            _config.TenantId,
            _config.ClientId,
            _config.ClientSecret
        );

        _armClient = new ArmClient(credential);
    }

    private ApiManagementServiceResource GetApimService()
    {
        if (_apimService != null)
            return _apimService;

        var resourceId = ApiManagementServiceResource.CreateResourceIdentifier(
            _config.SubscriptionId,
            _config.ResourceGroupName,
            _config.ApiManagementServiceName
        );

        _apimService = _armClient.GetApiManagementServiceResource(resourceId);
        return _apimService;
    }

    public async Task<List<ApiInfo>> GetApisAsync()
    {
        try
        {
            var apimService = GetApimService();
            var apis = apimService.GetApis();
            var apiList = new List<ApiInfo>();

            await foreach (var api in apis.GetAllAsync())
            {
                apiList.Add(new ApiInfo
                {
                    Id = api.Data.Name,
                    Name = api.Data.Name,
                    DisplayName = api.Data.DisplayName ?? string.Empty,
                    Path = api.Data.Path ?? string.Empty,
                    Description = api.Data.Description,
                    ServiceUrl = api.Data.ServiceUri?.ToString() ?? string.Empty,
                    Protocols = api.Data.Protocols?.Select(p => p.ToString()).ToList() ?? new List<string>()
                });
            }

            return apiList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving APIs from Azure API Management");
            throw;
        }
    }

    public async Task<ApiInfo?> GetApiAsync(string id)
    {
        try
        {
            var apimService = GetApimService();
            var api = await apimService.GetApis().GetAsync(id);

            if (api?.Value == null)
                return null;

            var apiData = api.Value.Data;
            return new ApiInfo
            {
                Id = apiData.Name,
                Name = apiData.Name,
                DisplayName = apiData.DisplayName ?? string.Empty,
                Path = apiData.Path ?? string.Empty,
                Description = apiData.Description,
                ServiceUrl = apiData.ServiceUri?.ToString() ?? string.Empty,
                Protocols = apiData.Protocols?.Select(p => p.ToString()).ToList() ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API {Id} from Azure API Management", id);
            throw;
        }
    }

    public async Task<ApiInfo> CreateApiAsync(ApiInfo api)
    {
        try
        {
            var apimService = GetApimService();

            var apiData = new ApiCreateOrUpdateContent
            {
                DisplayName = api.DisplayName,
                Path = api.Path,
                Description = api.Description,
                Protocols =
                {
                    ApiOperationInvokableProtocol.Https,
                    ApiOperationInvokableProtocol.Http,
                },
                ServiceUri = string.IsNullOrEmpty(api.ServiceUrl) ? null : new Uri(api.ServiceUrl)
            };
                       
            var result = await apimService.GetApis().CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                api.Name,
                apiData
            );

            var createdApi = result.Value;
            return new ApiInfo
            {
                Id = createdApi.Data.Name,
                Name = createdApi.Data.Name,
                DisplayName = createdApi.Data.DisplayName ?? string.Empty,
                Path = createdApi.Data.Path ?? string.Empty,
                Description = createdApi.Data.Description,
                ServiceUrl = createdApi.Data.ServiceUri?.ToString() ?? string.Empty,
                Protocols = createdApi.Data.Protocols?.Select(p => p.ToString()).ToList() ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API {Name} in Azure API Management", api.Name);
            throw;
        }
    }

    public async Task DeleteApiAsync(string id)
    {
        try
        {
            var apimService = GetApimService();
            var api = await apimService.GetApis().GetAsync(id);

            if (api?.Value != null)
            {
                await api.Value.DeleteAsync(Azure.WaitUntil.Completed, Azure.ETag.All);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting API {Id} from Azure API Management", id);
            throw;
        }
    }

    private async Task<string> GetApiExportLinkAsync(string subscriptionId,
                                          string resourceGroupName,
                                          string serviceName,
                                          string apiId)
    {
        var credential = new ClientSecretCredential(_config.TenantId, _config.ClientId, _config.ClientSecret);
        var token = await credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://management.azure.com/.default" }));
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);

        string apiVersion = "2024-05-01"; // or whichever version you target
        string format = "swagger-link-json"; // or openapi-link, etc
        string url = $"https://management.azure.com/subscriptions/{subscriptionId}" +
                     $"/resourceGroups/{resourceGroupName}/providers/Microsoft.ApiManagement" +
                     $"/service/{serviceName}/apis/{apiId}" +
                     $"?export=true&format={format}&api-version={apiVersion}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        // parse json to get link: json.value.link
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        string link = doc.RootElement.GetProperty("value").GetProperty("link").GetString();
        return link;
    }

    public async Task<string?> GetApiDefinitionAsync(string id)
    {
        try
        {
            var url = await GetApiExportLinkAsync(_config.SubscriptionId, _config.ResourceGroupName, _config.ApiManagementServiceName, id);
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var definition = await response.Content.ReadAsStringAsync();
                return definition;
            }
            _logger.LogWarning("No OpenAPI/Swagger schema found for API {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API definition for {Id} from Azure API Management", id);
            throw;
        }
    }

    public async Task<List<SubscriptionInfo>> GetSubscriptionsAsync()
    {
        try
        {
            var apimService = GetApimService();
            var subscriptions = apimService.GetApiManagementSubscriptions();
            var subscriptionList = new List<SubscriptionInfo>();

            await foreach (var subscription in subscriptions.GetAllAsync())
            {
                subscriptionList.Add(new SubscriptionInfo
                {
                    Id = subscription.Data.Name,
                    Name = subscription.Data.Name,
                    DisplayName = subscription.Data.DisplayName ?? string.Empty,
                    Scope = subscription.Data.Scope ?? string.Empty,
                    State = subscription.Data.State?.ToString() ?? string.Empty,
                    CreatedDate = subscription.Data.CreatedOn?.DateTime,
                    ExpirationDate = subscription.Data.ExpireOn?.DateTime
                });
            }

            return subscriptionList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscriptions from Azure API Management");
            throw;
        }
    }

    public async Task<SubscriptionInfo?> GetSubscriptionAsync(string id)
    {
        try
        {
            var apimService = GetApimService();
            var subscription = await apimService.GetApiManagementSubscriptions().GetAsync(id);

            if (subscription?.Value == null)
                return null;

            var subData = subscription.Value.Data;

            // Get subscription keys
            var keys = await subscription.Value.GetSecretsAsync();
            
            return new SubscriptionInfo
            {
                Id = subData.Name,
                Name = subData.Name,
                DisplayName = subData.DisplayName ?? string.Empty,
                Scope = subData.Scope ?? string.Empty,
                State = subData.State?.ToString() ?? string.Empty,
                PrimaryKey = keys?.Value?.PrimaryKey,
                SecondaryKey = keys?.Value?.SecondaryKey,
                CreatedDate = subData.CreatedOn?.DateTime,
                ExpirationDate = subData.ExpireOn?.DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription {Id} from Azure API Management", id);
            throw;
        }
    }

    public async Task<SubscriptionInfo> CreateSubscriptionAsync(SubscriptionInfo subscription)
    {
        try
        {
            var apimService = GetApimService();

            var subscriptionData = new ApiManagementSubscriptionCreateOrUpdateContent
            {
                DisplayName = subscription.DisplayName,
                Scope = subscription.Scope
            };

            var result = await apimService.GetApiManagementSubscriptions().CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                subscription.Name,
                subscriptionData
            );

            var createdSub = result.Value;
            return new SubscriptionInfo
            {
                Id = createdSub.Data.Name,
                Name = createdSub.Data.Name,
                DisplayName = createdSub.Data.DisplayName ?? string.Empty,
                Scope = createdSub.Data.Scope ?? string.Empty,
                State = createdSub.Data.State?.ToString() ?? string.Empty,
                CreatedDate = createdSub.Data.CreatedOn?.DateTime,
                ExpirationDate = createdSub.Data.ExpireOn?.DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription {Name} in Azure API Management", subscription.Name);
            throw;
        }
    }

    public async Task RegenerateSubscriptionKeyAsync(string id, string keyType)
    {
        try
        {
            var apimService = GetApimService();
            var subscription = await apimService.GetApiManagementSubscriptions().GetAsync(id);

            if (subscription?.Value != null)
            {
                if (keyType.Equals("primary", StringComparison.OrdinalIgnoreCase))
                {
                    await subscription.Value.RegeneratePrimaryKeyAsync();
                }
                else if (keyType.Equals("secondary", StringComparison.OrdinalIgnoreCase))
                {
                    await subscription.Value.RegenerateSecondaryKeyAsync();
                }
                else
                {
                    throw new ArgumentException($"Invalid key type: {keyType}. Must be 'primary' or 'secondary'.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating {KeyType} key for subscription {Id}", keyType, id);
            throw;
        }
    }
}
