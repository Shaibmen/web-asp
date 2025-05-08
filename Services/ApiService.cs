using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using WEbAPi.Models;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(IHttpClientFactory httpClientFactory,
                     IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<HttpClient> GetAuthorizedClientAsync()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        return _httpClient;
    }

    // Методы аутентификации
    public async Task<string?> LoginAsync(string login, string password)
    {
        var requestData = new { Login = login, Password = password };
        var response = await _httpClient.PostAsJsonAsync("api/AuthApi/login", requestData);

        if (!response.IsSuccessStatusCode)
            return null;

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse?.Token;
    }

    public async Task<string?> RegisterAsync(string login, string? email, string password)
    {
        var registerRequest = new { Login = login, Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync("api/AuthApi/register", registerRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {errorContent}");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse?.Token;
    }

    // Методы для каталога
    public async Task<List<WEbAPi.Models.Catalog>> GetCatalogAsync(string category, string searchQuery, string sortBy)
    {
        var url = $"api/Customer/catalog?category={WebUtility.UrlEncode(category)}" +
                 $"&searchQuery={WebUtility.UrlEncode(searchQuery)}" +
                 $"&sortBy={WebUtility.UrlEncode(sortBy)}";

        var response = await _httpClient.GetFromJsonAsync<List<WEbAPi.Models.Catalog>>(url, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });

        return response ?? new List<WEbAPi.Models.Catalog>();
    }

    // Единственная версия метода GetCatalogsAsync
    public async Task<List<WEbAPi.Models.Catalog>> GetCatalogsAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<WEbAPi.Models.Catalog>>("api/admin/catalogs")
               ?? new List<WEbAPi.Models.Catalog>();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var catalog = await GetCatalogAsync("", "", "");
        return catalog?
            .SelectMany(item => item.Categories)
            .GroupBy(c => c.CategoryId)
            .Select(g => g.First())
            .ToList() ?? new List<Category>();
    }

    // Методы для корзины
    public async Task<bool> AddToCartAsync(int catalogId)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/Customer/add-to-cart", new { catalogId });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PosOrder>> GetCartAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<PosOrder>>("api/Customer/cart")
               ?? new List<PosOrder>();
    }

    // Метод для получения пользователей
    public async Task<List<User>> GetUsersAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<User>>("api/admin/users")
               ?? new List<User>();
    }
}

// Модели данных
public class AuthResponse
{
    public string Token { get; set; }
}



public class PosOrder
{
    public int Id { get; set; }
    public int CatalogId { get; set; }
    public int Quantity { get; set; }
}

public class User
{
    public int UserId { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public int RoleId { get; set; }
}