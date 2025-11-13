using Microsoft.AspNetCore.Mvc;
using ApimGuard.Models;
using ApimGuard.Services;
using Microsoft.Extensions.Options;

namespace ApimGuard.Controllers;

public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IApiManagementService _apiManagementService;
    private readonly FeatureFlags _featureFlags;

    public ProductsController(ILogger<ProductsController> logger, IApiManagementService apiManagementService, IOptions<FeatureFlags> featureFlags)
    {
        _logger = logger;
        _apiManagementService = apiManagementService;
        _featureFlags = featureFlags.Value;
    }

    // List all Products
    public async Task<IActionResult> Index()
    {
        ViewBag.EnableDeleteOperations = _featureFlags.EnableDeleteOperations;
        try
        {
            var products = await _apiManagementService.GetProductsAsync();
            return View(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return View(new List<ProductInfo>());
        }
    }

    // Product details
    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var product = await _apiManagementService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            // Get linked APIs for this product
            var linkedApis = await _apiManagementService.GetProductApisAsync(id);
            ViewBag.LinkedApis = linkedApis;
            
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            return NotFound();
        }
    }

    // Create new Product - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create new Product - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductInfo product)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _apiManagementService.CreateProductAsync(product);
                _logger.LogInformation("Created product: {DisplayName}", product.DisplayName);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {DisplayName}", product.DisplayName);
                ModelState.AddModelError("", "Error creating product. Please try again.");
            }
        }
        return View(product);
    }

    // Delete Product
    public async Task<IActionResult> Delete(string id)
    {
        if (!_featureFlags.EnableDeleteOperations)
        {
            return NotFound();
        }

        try
        {
            var product = await _apiManagementService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id} for deletion", id);
            return NotFound();
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (!_featureFlags.EnableDeleteOperations)
        {
            return NotFound();
        }

        try
        {
            await _apiManagementService.DeleteProductAsync(id);
            _logger.LogInformation("Deleted product with ID: {Id}", id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
