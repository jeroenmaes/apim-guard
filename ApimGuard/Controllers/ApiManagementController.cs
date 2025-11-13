using Microsoft.AspNetCore.Mvc;
using ApimGuard.Models;

namespace ApimGuard.Controllers;

public class ApiManagementController : Controller
{
    private readonly ILogger<ApiManagementController> _logger;

    public ApiManagementController(ILogger<ApiManagementController> logger)
    {
        _logger = logger;
    }

    // List all APIs
    public IActionResult Index()
    {
        // This will be connected to Azure API Management in the future
        var apis = new List<ApiInfo>
        {
            new ApiInfo
            {
                Id = "sample-api-1",
                Name = "sample-api",
                DisplayName = "Sample API",
                Path = "/api/sample",
                Description = "A sample API for demonstration",
                ServiceUrl = "https://backend.example.com",
                Protocols = new List<string> { "https" }
            }
        };

        return View(apis);
    }

    // API details
    public IActionResult Details(string id)
    {
        var api = new ApiInfo
        {
            Id = id,
            Name = "sample-api",
            DisplayName = "Sample API",
            Path = "/api/sample",
            Description = "A sample API for demonstration",
            ServiceUrl = "https://backend.example.com",
            Protocols = new List<string> { "https" }
        };

        return View(api);
    }

    // Create new API - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create new API - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ApiInfo api)
    {
        if (ModelState.IsValid)
        {
            // Logic to create API in Azure API Management will be added here
            _logger.LogInformation($"Creating API: {api.DisplayName}");
            return RedirectToAction(nameof(Index));
        }
        return View(api);
    }

    // Delete API
    public IActionResult Delete(string id)
    {
        var api = new ApiInfo
        {
            Id = id,
            Name = "sample-api",
            DisplayName = "Sample API"
        };

        return View(api);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(string id)
    {
        // Logic to delete API from Azure API Management will be added here
        _logger.LogInformation($"Deleting API with ID: {id}");
        return RedirectToAction(nameof(Index));
    }
}
