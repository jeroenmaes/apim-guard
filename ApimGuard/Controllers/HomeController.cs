using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApimGuard.Models;

namespace ApimGuard.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult UserProfile()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SaveThemePreference(string theme)
    {
        if (theme != "light" && theme != "dark")
        {
            return BadRequest("Invalid theme value");
        }

        // Set cookie for 1 year
        Response.Cookies.Append("theme", theme, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = false, // Allow JavaScript to read the cookie
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });

        return Ok(new { success = true, theme = theme });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
