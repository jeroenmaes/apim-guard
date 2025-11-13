using Microsoft.AspNetCore.Mvc;
using ApimGuard.Models;

namespace ApimGuard.Controllers;

public class SubscriptionsController : Controller
{
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(ILogger<SubscriptionsController> logger)
    {
        _logger = logger;
    }

    // List all subscriptions
    public IActionResult Index()
    {
        // This will be connected to Azure API Management in the future
        var subscriptions = new List<SubscriptionInfo>
        {
            new SubscriptionInfo
            {
                Id = "sub-1",
                Name = "sample-subscription",
                DisplayName = "Sample Subscription",
                Scope = "/apis/sample-api",
                State = "active",
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            }
        };

        return View(subscriptions);
    }

    // Subscription details
    public IActionResult Details(string id)
    {
        var subscription = new SubscriptionInfo
        {
            Id = id,
            Name = "sample-subscription",
            DisplayName = "Sample Subscription",
            Scope = "/apis/sample-api",
            State = "active",
            PrimaryKey = "****",
            SecondaryKey = "****",
            CreatedDate = DateTime.UtcNow.AddDays(-30)
        };

        return View(subscription);
    }

    // Create new subscription - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create new subscription - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SubscriptionInfo subscription)
    {
        if (ModelState.IsValid)
        {
            // Logic to create subscription in Azure API Management will be added here
            _logger.LogInformation($"Creating subscription: {subscription.DisplayName}");
            return RedirectToAction(nameof(Index));
        }
        return View(subscription);
    }

    // Regenerate keys
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegenerateKey(string id, string keyType)
    {
        // Logic to regenerate subscription key in Azure API Management will be added here
        _logger.LogInformation($"Regenerating {keyType} key for subscription: {id}");
        return RedirectToAction(nameof(Details), new { id });
    }
}
