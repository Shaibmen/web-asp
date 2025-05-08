using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CustomerController : Controller
{
    private readonly ApiService _apiService;

    public CustomerController(ApiService apiService)
    {
        _apiService = apiService;
    }

    // Каталог товаров
    public async Task<IActionResult> Catalog(string category, string searchQuery, string sortBy)
    {
        var products = await _apiService.GetCatalogAsync(category, searchQuery, sortBy);
        ViewBag.Categories = await _apiService.GetCategoriesAsync(); // Если нужно
        return View(products);
    }

    // Корзина
    public async Task<IActionResult> Cart()
    {
        var cartItems = await _apiService.GetCartAsync();
        return View(cartItems);
    }

    // Добавление в корзину
    [HttpPost]
    public async Task<IActionResult> AddToCart(int catalogId)
    {
        var success = await _apiService.AddToCartAsync(catalogId);
        if (!success)
        {
            TempData["ErrorMessage"] = "Ошибка добавления в корзину";
        }
        return RedirectToAction("Catalog");
    }
}