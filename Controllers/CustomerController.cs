using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEbAPi.Models;
using System.Text.Json;
using System.Net.Http.Headers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ApiService _apiService;

    public CustomerController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Catalog(string category, string searchQuery, string sortBy)
    {
        var products = await _apiService.GetCatalogAsync(category, searchQuery, sortBy);
        ViewBag.Categories = await _apiService.GetCategoriesAsync();
        return View(products);
    }

    [HttpGet("Customer/ProductDetails/{id}")]
    public async Task<IActionResult> ProductDetails(int id)
    {
        Console.WriteLine($"Requested product ID: {id}");

        var product = await _apiService.GetProductDetailsAsync(id);
        if (product == null)
        {
            Console.WriteLine($"Product with ID {id} not found");
            return RedirectToAction("Catalog");
        }

        Console.WriteLine($"Loaded product ID: {product.CatalogsId}");

        return View(new ProductDetailsViewModel
        {
            Product = product,
            Reviews = await _apiService.GetProductReviewsAsync(id)
        });
    }

    [HttpPost("Customer/AddReview")]
    public async Task<IActionResult> AddReview(ReviewFormModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("ProductDetails", new { id = model.ProductId });

        var result = await _apiService.AddReviewAsync(model);
        if (!result)
            TempData["ErrorMessage"] = "Ошибка при добавлении отзыва";

        return RedirectToAction("ProductDetails", new { id = model.ProductId });
    }

    public async Task<IActionResult> Cart()
    {
        try
        {
            var cartResponse = await _apiService.GetCartAsync();

            if (cartResponse.Items == null || !cartResponse.Items.Any())
            {
                ViewBag.EmptyCartMessage = "Ваша корзина пуста";
                return View(new CartResponse { Items = new List<PosOrder>(), TotalSum = 0 });
            }

            if (cartResponse.TotalSum == 0)
            {
                cartResponse.TotalSum = cartResponse.Items.Sum(c =>
                    c.Count * (decimal.TryParse(c.Product?.Price, out var price) ? price : 0m));
            }

            return View(cartResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Cart: {ex.Message}");
            ViewBag.ErrorMessage = "Произошла ошибка при загрузке корзины";
            return View(new CartResponse { Items = new List<PosOrder>(), TotalSum = 0 });
        }
    }

    [HttpPost]
    [Route("Customer/AddToCart/{catalogId:int}")]
    public async Task<IActionResult> AddToCart(int catalogId)
    {
        Console.WriteLine($"AddToCart received catalogId: {catalogId}");

        if (catalogId <= 0)
        {
            Console.WriteLine($"Invalid catalogId: {catalogId}");
            TempData["ErrorMessage"] = "Неверный идентификатор товара";
            return RedirectToAction("Catalog");
        }

        try
        {
            var success = await _apiService.AddToCartAsync(catalogId);
            if (!success)
                TempData["ErrorMessage"] = "Ошибка добавления в корзину";

            return RedirectToAction("ProductDetails", new { id = catalogId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding to cart: {ex.Message}");
            TempData["ErrorMessage"] = "Произошла ошибка при добавлении в корзину";
            return RedirectToAction("ProductDetails", new { id = catalogId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(int posOrderId, int newCount)
    {
        try
        {
            var success = await _apiService.UpdateCartItemAsync(posOrderId, newCount);
            if (!success)
                TempData["ErrorMessage"] = "Ошибка обновления корзины";

            return RedirectToAction("Cart");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating cart: {ex.Message}");
            TempData["ErrorMessage"] = "Произошла ошибка при обновлении корзины";
            return RedirectToAction("Cart");
        }
    }
}