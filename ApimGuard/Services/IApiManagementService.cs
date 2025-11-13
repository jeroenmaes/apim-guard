using ApimGuard.Models;

namespace ApimGuard.Services;

public interface IApiManagementService
{
    // API Management
    Task<List<ApiInfo>> GetApisAsync();
    Task<ApiInfo?> GetApiAsync(string id);
    Task<ApiInfo> CreateApiAsync(ApiInfo api);
    Task<ApiInfo> CreateApiFromSpecificationAsync(ApiInfo api, Stream specificationContent, string specificationFormat);
    Task DeleteApiAsync(string id);
    Task<string?> GetApiDefinitionAsync(string id);

    // Subscription Management
    Task<List<SubscriptionInfo>> GetSubscriptionsAsync();
    Task<SubscriptionInfo?> GetSubscriptionAsync(string id);
    Task<SubscriptionInfo> CreateSubscriptionAsync(SubscriptionInfo subscription);
    Task RegenerateSubscriptionKeyAsync(string id, string keyType);

    // Product Management
    Task<List<ProductInfo>> GetProductsAsync();
    Task<ProductInfo?> GetProductAsync(string id);
    Task<ProductInfo> CreateProductAsync(ProductInfo product);
    Task DeleteProductAsync(string id);
    Task<List<ProductInfo>> GetApiProductsAsync(string apiId);
    Task<List<ApiInfo>> GetProductApisAsync(string productId);
}
