using Microsoft.AspNetCore.Mvc;
using ApimGuard.Models;

namespace ApimGuard.Controllers;

public class AppRegistrationsController : Controller
{
    private readonly ILogger<AppRegistrationsController> _logger;

    public AppRegistrationsController(ILogger<AppRegistrationsController> logger)
    {
        _logger = logger;
    }

    // List all app registrations
    public IActionResult Index()
    {
        // This will be connected to Microsoft Graph API in the future
        var apps = new List<AppRegistrationInfo>
        {
            new AppRegistrationInfo
            {
                Id = "app-1",
                AppId = "12345678-1234-1234-1234-123456789012",
                DisplayName = "Sample App Registration",
                CreatedDateTime = DateTime.UtcNow.AddDays(-60),
                HasSecrets = true,
                HasCertificates = false
            }
        };

        return View(apps);
    }

    // App registration details
    public IActionResult Details(string id)
    {
        var app = new AppRegistrationInfo
        {
            Id = id,
            AppId = "12345678-1234-1234-1234-123456789012",
            DisplayName = "Sample App Registration",
            CreatedDateTime = DateTime.UtcNow.AddDays(-60),
            RedirectUris = new List<string> { "https://localhost:5001/signin-oidc" },
            HasSecrets = true,
            HasCertificates = false
        };

        return View(app);
    }

    // Create new app registration - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create new app registration - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AppRegistrationInfo app)
    {
        if (ModelState.IsValid)
        {
            // Logic to create app registration in Entra ID will be added here
            _logger.LogInformation($"Creating app registration: {app.DisplayName}");
            return RedirectToAction(nameof(Index));
        }
        return View(app);
    }

    // Manage secrets for an app
    public IActionResult Secrets(string id)
    {
        var secrets = new List<AppSecretInfo>
        {
            new AppSecretInfo
            {
                Id = "secret-1",
                DisplayName = "Production Secret",
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddMonths(6)
            }
        };

        ViewBag.AppId = id;
        return View(secrets);
    }

    // Add new secret - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddSecret(string appId, string displayName, int validityMonths)
    {
        // Logic to add secret to app registration in Entra ID will be added here
        _logger.LogInformation($"Adding secret '{displayName}' to app: {appId}");
        return RedirectToAction(nameof(Secrets), new { id = appId });
    }

    // Manage certificates for an app
    public IActionResult Certificates(string id)
    {
        var certificates = new List<AppCertificateInfo>
        {
            new AppCertificateInfo
            {
                Id = "cert-1",
                DisplayName = "Production Certificate",
                Thumbprint = "ABC123DEF456",
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddYears(1)
            }
        };

        ViewBag.AppId = id;
        return View(certificates);
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
    public IActionResult UploadCertificate(string appId, IFormFile certificate)
    {
        if (certificate != null && certificate.Length > 0)
        {
            // Logic to upload certificate to app registration in Entra ID will be added here
            _logger.LogInformation($"Uploading certificate to app: {appId}");
        }
        return RedirectToAction(nameof(Certificates), new { id = appId });
    }

    // Delete app registration
    public IActionResult Delete(string id)
    {
        var app = new AppRegistrationInfo
        {
            Id = id,
            AppId = "12345678-1234-1234-1234-123456789012",
            DisplayName = "Sample App Registration"
        };

        return View(app);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(string id)
    {
        // Logic to delete app registration from Entra ID will be added here
        _logger.LogInformation($"Deleting app registration with ID: {id}");
        return RedirectToAction(nameof(Index));
    }
}
