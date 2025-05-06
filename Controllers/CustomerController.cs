using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WEbAPi.Models.Dto;
using WEbAPi.Models;

[Authorize]
public class CustomerController : Controller
{
    private readonly CustomerService _customerService;
    private readonly IHttpClientFactory _httpClientFactory;

    public CustomerController(CustomerService customerService, IHttpClientFactory httpClientFactory)
    {
        _customerService = customerService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Catalog(string searchQuery, string category, string sortBy)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        // Получение каталога
        var response = await client.GetAsync("api/CustomerApi/catalog");
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Categories = new List<Category>();
            return View(new List<Catalog>());
        }

        var json = await response.Content.ReadAsStringAsync();
        var catalogs = JsonConvert.DeserializeObject<List<Catalog>>(json) ?? new List<Catalog>();

        // Получение категорий
        var catResponse = await client.GetAsync("api/category");
        if (catResponse.IsSuccessStatusCode)
        {
            var catJson = await catResponse.Content.ReadAsStringAsync();
            var categoryList = JsonConvert.DeserializeObject<List<Category>>(catJson);
            ViewBag.Categories = categoryList ?? new List<Category>();
        }
        else
        {
            ViewBag.Categories = new List<Category>();
        }

        // Фильтрация
        if (!string.IsNullOrEmpty(searchQuery))
        {
            catalogs = catalogs.Where(c => c.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrEmpty(category))
        {
            catalogs = catalogs
                .Where(c => c.Categories != null && c.Categories.Any(cat => cat.CategoryName == category))
                .ToList();
        }

        catalogs = sortBy switch
        {
            "price_asc" => catalogs.OrderBy(c => c.Price).ToList(),
            "price_desc" => catalogs.OrderByDescending(c => c.Price).ToList(),
            _ => catalogs
        };

        return View(catalogs);
    }

    [HttpGet]
    public async Task<IActionResult> ProductDetails(int id)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.GetAsync($"api/CustomerApi/product/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var json = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<Catalog>(json);

        if (product == null)
        {
            return NotFound();
        }

        // Получаем отзывы
        var reviewsResponse = await client.GetAsync($"api/CustomerApi/reviews/{id}");
        var reviews = reviewsResponse.IsSuccessStatusCode
            ? JsonConvert.DeserializeObject<List<Review>>(await reviewsResponse.Content.ReadAsStringAsync())
            : new List<Review>();

        var viewModel = new ProductDetailsViewModel
        {
            Product = product,
            Reviews = reviews
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddReview(ReviewFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Некорректные данные.");
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PostAsJsonAsync("api/CustomerApi/add-review", model);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction("ProductDetails", new { id = model.ProductId });
        }

        return RedirectToAction("ProductDetails", new { id = model.ProductId });
    }

    [HttpGet]
    public async Task<IActionResult> Cart()
    {
        try
        {
            var (items, totalSum) = await _customerService.GetCartAsync();
            return View((items, totalSum));
        }
        catch
        {
            return RedirectToAction("Login", "Auth");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int catalogId)
    {
        var result = await _customerService.AddToCartAsync(new AddToCartDto { CatalogId = catalogId });
        if (result == null)
            return RedirectToAction("Cart");

        TempData["Error"] = result;
        return RedirectToAction("Catalog");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(int posOrderId, int newCount)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PostAsJsonAsync("api/CustomerApi/update-cart-item",
            new { posOrderId, newCount });

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
        }

        return RedirectToAction("Cart");
    }
}