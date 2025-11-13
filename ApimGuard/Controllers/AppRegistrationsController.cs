using Microsoft.AspNetCore.Mvc;
using ApimGuard.Models;
using ApimGuard.Services;

namespace ApimGuard.Controllers;

public class AppRegistrationsController : Controller
{
    private readonly ILogger<AppRegistrationsController> _logger;
    private readonly IGraphApiService _graphApiService;

    public AppRegistrationsController(ILogger<AppRegistrationsController> logger, IGraphApiService graphApiService)
    {
        _logger = logger;
        _graphApiService = graphApiService;
    }

    // List all app registrations
    public async Task<IActionResult> Index()
    {
        try
        {
            var apps = await _graphApiService.GetApplicationsAsync();
            return View(apps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving app registrations");
            TempData["Error"] = "Failed to retrieve app registrations. Please check your Azure configuration.";
            return View(new List<AppRegistrationInfo>());
        }
    }

    // App registration details
    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var app = await _graphApiService.GetApplicationAsync(id);
            if (app == null)
            {
                return NotFound();
            }
            return View(app);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving app registration details for {Id}", id);
            TempData["Error"] = "Failed to retrieve app registration details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // Create new app registration - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create new app registration - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppRegistrationInfo app)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var createdApp = await _graphApiService.CreateApplicationAsync(app.DisplayName, app.RedirectUris);
                _logger.LogInformation("Created app registration: {DisplayName} with ID: {Id}", createdApp.DisplayName, createdApp.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating app registration: {DisplayName}", app.DisplayName);
                ModelState.AddModelError(string.Empty, "Failed to create app registration. Please try again.");
            }
        }
        return View(app);
    }

    // Manage secrets for an app
    public async Task<IActionResult> Secrets(string id)
    {
        try
        {
            var secrets = await _graphApiService.GetApplicationSecretsAsync(id);
            ViewBag.AppId = id;
            return View(secrets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secrets for app {Id}", id);
            TempData["Error"] = "Failed to retrieve app secrets.";
            return RedirectToAction(nameof(Index));
        }
    }

    // Add new secret - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSecret(string appId, string displayName, int validityMonths)
    {
        try
        {
            var secret = await _graphApiService.AddApplicationSecretAsync(appId, displayName, validityMonths);
            _logger.LogInformation("Added secret '{DisplayName}' to app: {AppId}", displayName, appId);
            TempData["SecretValue"] = secret.SecretValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding secret to app {AppId}", appId);
            TempData["Error"] = "Failed to add secret. Please try again.";
        }
        return RedirectToAction(nameof(Secrets), new { id = appId });
    }

    // Manage certificates for an app
    public async Task<IActionResult> Certificates(string id)
    {
        try
        {
            var certificates = await _graphApiService.GetApplicationCertificatesAsync(id);
            ViewBag.AppId = id;
            return View(certificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving certificates for app {Id}", id);
            TempData["Error"] = "Failed to retrieve app certificates.";
            return RedirectToAction(nameof(Index));
        }
    }

    // Upload certificate - GET
    public IActionResult UploadCertificate(string id)
    {
        ViewBag.AppId = id;
        return View();
    }

    // Upload certificate - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCertificate(string appId, IFormFile certificate)
    {
        if (certificate != null && certificate.Length > 0)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await certificate.CopyToAsync(memoryStream);
                var certificateData = memoryStream.ToArray();
                
                await _graphApiService.AddApplicationCertificateAsync(appId, certificateData, certificate.FileName);
                _logger.LogInformation("Uploaded certificate to app: {AppId}", appId);
                TempData["Success"] = "Certificate uploaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading certificate to app {AppId}", appId);
                TempData["Error"] = "Failed to upload certificate. Please try again.";
            }
        }
        return RedirectToAction(nameof(Certificates), new { id = appId });
    }

    // Delete app registration
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var app = await _graphApiService.GetApplicationAsync(id);
            if (app == null)
            {
                return NotFound();
            }
            return View(app);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving app registration for deletion {Id}", id);
            TempData["Error"] = "Failed to retrieve app registration.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            await _graphApiService.DeleteApplicationAsync(id);
            _logger.LogInformation("Deleted app registration with ID: {Id}", id);
            TempData["Success"] = "App registration deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting app registration {Id}", id);
            TempData["Error"] = "Failed to delete app registration. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }
}
