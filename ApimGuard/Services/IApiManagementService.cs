using ApimGuard.Models;

namespace ApimGuard.Services;

public interface IApiManagementService
{
    // API Management
    Task<List<ApiInfo>> GetApisAsync();
    Task<ApiInfo?> GetApiAsync(string id);
    Task<ApiInfo> CreateApiAsync(ApiInfo api);
    Task DeleteApiAsync(string id);
    Task<string?> GetApiDefinitionAsync(string id);

    // Subscription Management
    Task<List<SubscriptionInfo>> GetSubscriptionsAsync();
    Task<SubscriptionInfo?> GetSubscriptionAsync(string id);
    Task<SubscriptionInfo> CreateSubscriptionAsync(SubscriptionInfo subscription);
    Task RegenerateSubscriptionKeyAsync(string id, string keyType);
}
