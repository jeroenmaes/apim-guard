using Microsoft.AspNetCore.Mvc;
using ApimGuard.Models;
using ApimGuard.Services;

namespace ApimGuard.Controllers;

public class SubscriptionsController : Controller
{
    private readonly ILogger<SubscriptionsController> _logger;
    private readonly IApiManagementService _apiManagementService;

    public SubscriptionsController(ILogger<SubscriptionsController> logger, IApiManagementService apiManagementService)
    {
        _logger = logger;
        _apiManagementService = apiManagementService;
    }

    // List all subscriptions
    public async Task<IActionResult> Index()
    {
        try
        {
            var subscriptions = await _apiManagementService.GetSubscriptionsAsync();
            return View(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscriptions");
            return View(new List<SubscriptionInfo>());
        }
    }

    // Subscription details
    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var subscription = await _apiManagementService.GetSubscriptionAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }
            return View(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription {Id}", id);
            return NotFound();
        }
    }

    // Create new subscription - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create new subscription - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubscriptionInfo subscription)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _apiManagementService.CreateSubscriptionAsync(subscription);
                _logger.LogInformation($"Created subscription: {subscription.DisplayName}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription {DisplayName}", subscription.DisplayName);
                ModelState.AddModelError("", "Error creating subscription. Please try again.");
            }
        }
        return View(subscription);
    }

    // Regenerate keys
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerateKey(string id, string keyType)
    {
        try
        {
            await _apiManagementService.RegenerateSubscriptionKeyAsync(id, keyType);
            _logger.LogInformation($"Regenerated {keyType} key for subscription: {id}");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating {KeyType} key for subscription {Id}", keyType, id);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
