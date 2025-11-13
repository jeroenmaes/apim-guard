using Microsoft.AspNetCore.Mvc;
using ApimGuard.Models;
using ApimGuard.Services;

namespace ApimGuard.Controllers;

public class ApiManagementController : Controller
{
    private readonly ILogger<ApiManagementController> _logger;
    private readonly IApiManagementService _apiManagementService;

    public ApiManagementController(ILogger<ApiManagementController> logger, IApiManagementService apiManagementService)
    {
        _logger = logger;
        _apiManagementService = apiManagementService;
    }

    // List all APIs
    public async Task<IActionResult> Index()
    {
        try
        {
            var apis = await _apiManagementService.GetApisAsync();
            return View(apis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving APIs");
            return View(new List<ApiInfo>());
        }
    }

    // API details
    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var api = await _apiManagementService.GetApiAsync(id);
            if (api == null)
            {
                return NotFound();
            }
            return View(api);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API {Id}", id);
            return NotFound();
        }
    }

    // Create new API - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create new API - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApiInfo api)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _apiManagementService.CreateApiAsync(api);
                _logger.LogInformation($"Created API: {api.DisplayName}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API {DisplayName}", api.DisplayName);
                ModelState.AddModelError("", "Error creating API. Please try again.");
            }
        }
        return View(api);
    }

    // Delete API
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var api = await _apiManagementService.GetApiAsync(id);
            if (api == null)
            {
                return NotFound();
            }
            return View(api);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API {Id} for deletion", id);
            return NotFound();
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            await _apiManagementService.DeleteApiAsync(id);
            _logger.LogInformation($"Deleted API with ID: {id}");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting API {Id}", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
