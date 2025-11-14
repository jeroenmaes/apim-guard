using ApimGuard.Models;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager.ApiManagement.Models;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
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
                    Protocols = api.Data.Protocols?.Select(p => p.ToString()).ToList() ?? new List<string>(),
                    SubscriptionRequired = api.Data.IsSubscriptionRequired ?? false
                    // Note: AzureAdApplicationIds not populated in list view for performance reasons
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
            
            // Get Azure AD application IDs, audiences, and required claims from policy
            var (azureAdClientAppIds, audiences, requiredClaims) = await GetAzureAdSecurityDetailsFromPolicyAsync(api.Value);
            
            return new ApiInfo
            {
                Id = apiData.Name,
                Name = apiData.Name,
                DisplayName = apiData.DisplayName ?? string.Empty,
                Path = apiData.Path ?? string.Empty,
                Description = apiData.Description,
                ServiceUrl = apiData.ServiceUri?.ToString() ?? string.Empty,
                Protocols = apiData.Protocols?.Select(p => p.ToString()).ToList() ?? new List<string>(),
                SubscriptionRequired = apiData.IsSubscriptionRequired ?? false,
                AzureAdClientApplicationIds = azureAdClientAppIds,
                Audiences = audiences,
                RequiredClaims = requiredClaims
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

    public async Task<ApiInfo> CreateApiFromSpecificationAsync(ApiInfo api, Stream specificationContent, string specificationFormat)
    {
        try
        {
            var apimService = GetApimService();

            // Read the specification content
            using var reader = new StreamReader(specificationContent);
            var specContent = await reader.ReadToEndAsync();

            // Determine the content format and value
            var contentFormat = MapSpecificationFormat(specificationFormat);
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
                Format = contentFormat,
                Value = specContent,
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
            _logger.LogError(ex, "Error creating API {Name} from specification in Azure API Management", api.Name);
            throw;
        }
    }

    private ContentFormat MapSpecificationFormat(string format)
    {
        return format?.ToLowerInvariant() switch
        {
            "openapi" => ContentFormat.OpenApi,
            "openapi+json" => ContentFormat.OpenApiJson,
            "openapi+json-link" => ContentFormat.OpenApiJsonLink,
            "openapi-link" => ContentFormat.OpenApiLink,
            "swagger-json" => ContentFormat.SwaggerJson,
            "swagger-link-json" => ContentFormat.SwaggerLinkJson,
            "wadl-link-json" => ContentFormat.WadlLinkJson,
            "wadl-xml" => ContentFormat.WadlXml,
            "wsdl" => ContentFormat.Wsdl,
            "wsdl-link" => ContentFormat.WsdlLink,
            "graphql-link" => ContentFormat.GraphQLLink,
            "odata" => ContentFormat.Odata,
            "odata-link" => ContentFormat.OdataLink,
            "grpc" => ContentFormat.Grpc,
            "grpc-link" => ContentFormat.GrpcLink,
            _ => ContentFormat.OpenApiJson // Default to OpenAPI JSON
        };
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

    public async Task DeleteSubscriptionAsync(string id)
    {
        try
        {
            var apimService = GetApimService();
            var subscription = await apimService.GetApiManagementSubscriptions().GetAsync(id);

            if (subscription?.Value != null)
            {
                await subscription.Value.DeleteAsync(Azure.WaitUntil.Completed, Azure.ETag.All);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription {Id} from Azure API Management", id);
            throw;
        }
    }

    private async Task<(List<string> applicationIds, List<string> audiences, List<RequiredClaim> requiredClaims)> GetAzureAdSecurityDetailsFromPolicyAsync(ApiResource api)
    {
        var applicationIds = new List<string>();
        var audiences = new List<string>();
        var requiredClaims = new List<RequiredClaim>();
        
        try
        {
            // Get the API policy
            var policies = api.GetApiPolicies();
            await foreach (var policy in policies.GetAllAsync())
            {
                if (policy.Data.Value != null)
                {
                    // Parse the policy XML to find validate-azure-ad-token elements
                    var (appIds, auds, claims) = ParseAzureAdSecurityDetails(policy.Data.Value);
                    applicationIds.AddRange(appIds);
                    audiences.AddRange(auds);
                    requiredClaims.AddRange(claims);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving API policy for {ApiId}", api.Data.Name);
            // Don't throw - just return empty lists if we can't get policies
        }
        
        return (applicationIds, audiences, requiredClaims);
    }

    private (List<string> applicationIds, List<string> audiences, List<RequiredClaim> requiredClaims) ParseAzureAdSecurityDetails(string policyXml)
    {
        var applicationIds = new List<string>();
        var audiences = new List<string>();
        var requiredClaims = new List<RequiredClaim>();
        
        try
        {
            var doc = XDocument.Parse(policyXml);
            
            // Find all validate-azure-ad-token elements
            var validateTokenElements = doc.Descendants()
                .Where(e => e.Name.LocalName == "validate-azure-ad-token");
            
            foreach (var element in validateTokenElements)
            {
                // Look for client-application-ids wrapper and its application-id child elements
                var clientAppIdsElements = element.Descendants()
                    .Where(e => e.Name.LocalName == "client-application-ids");
                
                foreach (var clientAppIdsElement in clientAppIdsElements)
                {
                    var appIdElements = clientAppIdsElement.Descendants()
                        .Where(e => e.Name.LocalName == "application-id");
                    
                    foreach (var appIdElement in appIdElements)
                    {
                        var appId = appIdElement.Value?.Trim();
                        if (!string.IsNullOrEmpty(appId))
                        {
                            applicationIds.Add(appId);
                        }
                    }
                }
                
                // Look for audiences node and its audience child elements
                var audiencesElements = element.Descendants()
                    .Where(e => e.Name.LocalName == "audiences");
                
                foreach (var audiencesElement in audiencesElements)
                {
                    var audienceElements = audiencesElement.Descendants()
                        .Where(e => e.Name.LocalName == "audience");
                    
                    foreach (var audienceElement in audienceElements)
                    {
                        var audience = audienceElement.Value?.Trim();
                        if (!string.IsNullOrEmpty(audience))
                        {
                            audiences.Add(audience);
                        }
                    }
                }
                
                // Look for required-claims node and its claim child elements
                var requiredClaimsElements = element.Descendants()
                    .Where(e => e.Name.LocalName == "required-claims");
                
                foreach (var requiredClaimsElement in requiredClaimsElements)
                {
                    var claimElements = requiredClaimsElement.Descendants()
                        .Where(e => e.Name.LocalName == "claim");
                    
                    foreach (var claimElement in claimElements)
                    {
                        var claimName = claimElement.Attribute("name")?.Value?.Trim();
                        var claimMatch = claimElement.Attribute("match")?.Value?.Trim() ?? "all";
                        
                        if (!string.IsNullOrEmpty(claimName))
                        {
                            var claim = new RequiredClaim
                            {
                                Name = claimName,
                                Match = claimMatch
                            };
                            
                            // Get all value elements within the claim
                            var valueElements = claimElement.Descendants()
                                .Where(e => e.Name.LocalName == "value");
                            
                            foreach (var valueElement in valueElements)
                            {
                                var value = valueElement.Value?.Trim();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    claim.Values.Add(value);
                                }
                            }
                            
                            requiredClaims.Add(claim);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing policy XML for Azure AD security details");
        }
        
        return (applicationIds, audiences, requiredClaims);
    }

    public async Task<List<ProductInfo>> GetProductsAsync()
    {
        try
        {
            var apimService = GetApimService();
            var products = apimService.GetApiManagementProducts();
            var productList = new List<ProductInfo>();

            await foreach (var product in products.GetAllAsync())
            {
                productList.Add(new ProductInfo
                {
                    Id = product.Data.Name,
                    Name = product.Data.Name,
                    DisplayName = product.Data.DisplayName ?? string.Empty,
                    Description = product.Data.Description,
                    State = product.Data.State?.ToString() ?? string.Empty,
                    SubscriptionRequired = product.Data.IsSubscriptionRequired ?? false,
                    ApprovalRequired = product.Data.IsApprovalRequired ?? false,
                    SubscriptionsLimit = product.Data.SubscriptionsLimit,
                    Terms = product.Data.Terms
                });
            }

            return productList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products from Azure API Management");
            throw;
        }
    }

    public async Task<ProductInfo?> GetProductAsync(string id)
    {
        try
        {
            var apimService = GetApimService();
            var product = await apimService.GetApiManagementProducts().GetAsync(id);

            if (product?.Value == null)
                return null;

            var productData = product.Value.Data;

            return new ProductInfo
            {
                Id = productData.Name,
                Name = productData.Name,
                DisplayName = productData.DisplayName ?? string.Empty,
                Description = productData.Description,
                State = productData.State?.ToString() ?? string.Empty,
                SubscriptionRequired = productData.IsSubscriptionRequired ?? false,
                ApprovalRequired = productData.IsApprovalRequired ?? false,
                SubscriptionsLimit = productData.SubscriptionsLimit,
                Terms = productData.Terms
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id} from Azure API Management", id);
            throw;
        }
    }

    public async Task<ProductInfo> CreateProductAsync(ProductInfo product)
    {
        try
        {
            var apimService = GetApimService();

            var productData = new ApiManagementProductData
            {
                DisplayName = product.DisplayName,
                Description = product.Description,
                Terms = product.Terms,
                IsSubscriptionRequired = product.SubscriptionRequired,
                IsApprovalRequired = product.ApprovalRequired,
                SubscriptionsLimit = product.SubscriptionsLimit,
                State = string.IsNullOrEmpty(product.State) ? null : 
                    Enum.Parse<ApiManagementProductState>(product.State)
            };

            var result = await apimService.GetApiManagementProducts().CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                product.Name,
                productData
            );

            var createdProduct = result.Value;
            return new ProductInfo
            {
                Id = createdProduct.Data.Name,
                Name = createdProduct.Data.Name,
                DisplayName = createdProduct.Data.DisplayName ?? string.Empty,
                Description = createdProduct.Data.Description,
                State = createdProduct.Data.State?.ToString() ?? string.Empty,
                SubscriptionRequired = createdProduct.Data.IsSubscriptionRequired ?? false,
                ApprovalRequired = createdProduct.Data.IsApprovalRequired ?? false,
                SubscriptionsLimit = createdProduct.Data.SubscriptionsLimit,
                Terms = createdProduct.Data.Terms
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {Name} in Azure API Management", product.Name);
            throw;
        }
    }

    public async Task DeleteProductAsync(string id)
    {
        try
        {
            var apimService = GetApimService();
            var product = await apimService.GetApiManagementProducts().GetAsync(id);

            if (product?.Value != null)
            {
                await product.Value.DeleteAsync(Azure.WaitUntil.Completed, Azure.ETag.All, deleteSubscriptions: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id} from Azure API Management", id);
            throw;
        }
    }

    public async Task<List<ProductInfo>> GetApiProductsAsync(string apiId)
    {
        try
        {
            var apimService = GetApimService();
            var api = await apimService.GetApis().GetAsync(apiId);
            
            if (api?.Value == null)
                return new List<ProductInfo>();

            var apiProducts = api.Value.GetApiProducts();
            var productList = new List<ProductInfo>();

            foreach (var apiProduct in apiProducts)
            {
                // Get the product details
                var productData = apiProduct.Data;
                productList.Add(new ProductInfo
                {
                    Id = productData.Name,
                    Name = productData.Name,
                    DisplayName = productData.DisplayName ?? string.Empty,
                    Description = productData.Description,
                    State = productData.State?.ToString() ?? string.Empty,
                    SubscriptionRequired = productData.IsSubscriptionRequired ?? false,
                    ApprovalRequired = productData.IsApprovalRequired ?? false,
                    SubscriptionsLimit = productData.SubscriptionsLimit,
                    Terms = productData.Terms
                });
            }

            return productList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for API {ApiId} from Azure API Management", apiId);
            throw;
        }
    }

    public async Task<List<ApiInfo>> GetProductApisAsync(string productId)
    {
        try
        {
            var apimService = GetApimService();
            var product = await apimService.GetApiManagementProducts().GetAsync(productId);
            
            if (product?.Value == null)
                return new List<ApiInfo>();

            var productApis = product.Value.GetProductApis();
            var apiList = new List<ApiInfo>();

            foreach (var productApi in productApis)
            {
                // Get the API details
                apiList.Add(new ApiInfo
                {
                    Id = productApi.Name,
                    Name = productApi.Name,
                    DisplayName = productApi.DisplayName ?? string.Empty,
                    Path = productApi.Path ?? string.Empty,
                    Description = productApi.Description,
                    ServiceUrl = productApi.ServiceUri?.ToString() ?? string.Empty,
                    Protocols = productApi.Protocols?.Select(p => p.ToString()).ToList() ?? new List<string>(),
                    SubscriptionRequired = productApi.IsSubscriptionRequired ?? false
                });
            }

            return apiList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving APIs for product {ProductId} from Azure API Management", productId);
            throw;
        }
    }
}
