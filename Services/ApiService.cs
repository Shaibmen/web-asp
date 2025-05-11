using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using WEbAPi.Models;
using WEbAPi.Models.Dto;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(IHttpClientFactory httpClientFactory,
                     IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<HttpClient> GetAuthorizedClientAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }



    public async Task<User> GetUserAsync(string login)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.GetAsync($"api/AuthApi/user-by-login/{login}");

        if (!response.IsSuccessStatusCode)
            throw new Exception("User not found");

        var content = await response.Content.ReadFromJsonAsync<User>();
        return content;
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

    public async Task<(string Token, string Error)> RegisterAsync(string login, string? email, string password)
    {
        try
        {
            var registerRequest = new { Login = login, Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/AuthApi/register", registerRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, $"Ошибка регистрации: {errorContent}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return (authResponse?.Token, null);
        }
        catch (Exception ex)
        {
            return (null, $"Ошибка при регистрации: {ex.Message}");
        }
    }

    // Методы для каталога
    public async Task<List<Catalog>> GetCatalogAsync(string category, string searchQuery, string sortBy)
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var url = $"api/Customer/catalog?category={WebUtility.UrlEncode(category)}" +
                     $"&searchQuery={WebUtility.UrlEncode(searchQuery)}" +
                     $"&sortBy={WebUtility.UrlEncode(sortBy)}";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return new List<Catalog>();
            }

            return await response.Content.ReadFromJsonAsync<List<Catalog>>() ?? new List<Catalog>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetCatalogAsync: {ex.Message}");
            return new List<Catalog>();
        }
    }

    public async Task<List<Catalog>> GetCatalogsAsync()
    {
        try
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync("api/admin/catalogs");

            if (!response.IsSuccessStatusCode)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Детали ошибки: {errorContent}");
                return new List<Catalog>();
            }

            return await response.Content.ReadFromJsonAsync<List<Catalog>>()
                   ?? new List<Catalog>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Исключение: {ex.Message}");
            return new List<Catalog>();
        }
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

    public async Task<bool> AddToCartAsync(int catalogId)
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var response = await client.PostAsJsonAsync(
                "api/Customer/add-to-cart",
                new { CatalogId = catalogId });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка при добавлении в корзину: {error}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в AddToCartAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<CartResponse> GetCartAsync()
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var response = await client.GetAsync("api/Customer/cart");

            if (!response.IsSuccessStatusCode)
                return new CartResponse { Items = new List<PosOrder>(), TotalSum = 0 };

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CartResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new CartResponse { Items = new List<PosOrder>(), TotalSum = 0 };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting cart: {ex.Message}");
            return new CartResponse { Items = new List<PosOrder>(), TotalSum = 0 };
        }
    }

    public async Task<Catalog> GetProductDetailsAsync(int id)
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var response = await client.GetAsync($"api/customer/product-details/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {json}");

            var result = JsonSerializer.Deserialize<ProductApiResponse>(json, options);
            return result?.Product;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetProductDetailsAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Review>> GetProductReviewsAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"api/customer/product-details/{productId}");
        if (!response.IsSuccessStatusCode) return new List<Review>();

        var details = await response.Content.ReadFromJsonAsync<ProductDetailsResponse>();
        return details?.Reviews?.ToList() ?? new List<Review>();
    }

    public async Task<bool> AddReviewAsync(ReviewFormModel model)
    {
        var client = await GetAuthorizedClientAsync();

        var request = new
        {
            ProductId = model.ProductId,
            Text = model.Text,
            Rating = model.Rating
        };

        var response = await client.PostAsJsonAsync("api/customer/add-review", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<AverageRatingResponse>(
                $"api/Customer/average-rating/{productId}");
            return response?.AverageRating ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<User>>("api/admin/users")
               ?? new List<User>();
    }

    public HttpClient GetHttpClient()
    {
        return _httpClient;
    }

    public async Task<bool> UpdateCartItemAsync(int posOrderId, int newCount)
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var request = new UpdateCartRequest { PosOrderId = posOrderId, NewCount = newCount };

            var response = await client.PostAsJsonAsync("api/Customer/update-cart", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении корзины: {ex.Message}");
            return false;
        }
    }

    #region Admin Methods

    // Catalogs
    public async Task<bool> CreateCatalogAsync(Catalog catalog)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/admin/catalogs", catalog);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCatalogAsync(int id, Catalog catalog)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PutAsJsonAsync($"api/admin/catalogs/{id}", catalog);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCatalogAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.DeleteAsync($"api/admin/catalogs/{id}");
        return response.IsSuccessStatusCode;
    }

    // Categories
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<Category>>("api/admin/categories")
               ?? new List<Category>();
    }

    public async Task<Category> GetCategoryAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<Category>($"api/admin/categories/{id}");
    }

    public async Task<bool> CreateCategoryAsync(Category category)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/admin/categories", category);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCategoryAsync(int id, Category category)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PutAsJsonAsync($"api/admin/categories/{id}", category);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.DeleteAsync($"api/admin/categories/{id}");
        return response.IsSuccessStatusCode;
    }

    // Orders
    public async Task<List<Order>> GetOrdersAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<Order>>("api/admin/orders")
               ?? new List<Order>();
    }

    public async Task<Order> GetOrderAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<Order>($"api/admin/orders/{id}");
    }

    public async Task<bool> CreateOrderAsync(Order order)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/admin/orders", order);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateOrderAsync(int id, Order order)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PutAsJsonAsync($"api/admin/orders/{id}", order);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.DeleteAsync($"api/admin/orders/{id}");
        return response.IsSuccessStatusCode;
    }

    // PosOrders
    public async Task<PosOrder> GetPosOrderAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<PosOrder>($"api/admin/posorders/{id}");
    }

    public async Task<List<PosOrder>> GetPosOrdersAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<PosOrder>>("api/admin/posorders")
               ?? new List<PosOrder>();
    }

    public async Task<bool> CreatePosOrderAsync(PosOrder posOrder)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/admin/posorders", posOrder);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePosOrderAsync(int id, PosOrder posOrder)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PutAsJsonAsync($"api/admin/posorders/{id}", posOrder);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePosOrderAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.DeleteAsync($"api/admin/posorders/{id}");
        return response.IsSuccessStatusCode;
    }

    // Users
    public async Task<bool> CreateUserAsync(User user)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/admin/users", user);
        return response.IsSuccessStatusCode;
    }

    public async Task<User> GetUserAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<User>($"api/admin/users/{id}");
    }

    public async Task<bool> UpdateUserAsync(int id, User user)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.PutAsJsonAsync($"api/admin/users/{id}", user);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.DeleteAsync($"api/admin/users/{id}");
        return response.IsSuccessStatusCode;
    }

    // Roles
    public async Task<List<Role>> GetRolesAsync()
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<List<Role>>("api/admin/roles")
               ?? new List<Role>();
    }

    public async Task<Catalog> GetCatalogByIdAsync(int id)
    {
        var client = await GetAuthorizedClientAsync();
        return await client.GetFromJsonAsync<Catalog>($"api/admin/catalogs/{id}");
    }
    #endregion

    public async Task<HttpClient> GetAuthenticatedClient()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        // Получаем токен из куков
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            Console.WriteLine("JWT токен не найден в куках");
        }

        return client;
    }

    public async Task<List<Order>> GetOrdersWithDetailsAsync()
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var response = await client.GetAsync("api/admin/orders/with-details");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting orders: {response.StatusCode}");
                return new List<Order>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };

            return await response.Content.ReadFromJsonAsync<List<Order>>(options)
                   ?? new List<Order>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetOrdersWithDetailsAsync: {ex.Message}");
            return new List<Order>();
        }
    }



    public async Task<Role> GetRoleAsync(int id)
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var response = await client.GetAsync($"api/admin/roles/{id}");

            // Убрали EnsureSuccessStatusCode() для более гибкой обработки
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Доступ запрещен. Требуется авторизация.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Ошибка при получении роли: {response.StatusCode}. {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Role>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в GetRoleAsync: {ex.Message}");
            throw; // Пробрасываем исключение дальше
        }
    }

    public async Task<bool> UpdateRoleAsync(int id, Role role)
    {
        try
        {
            var client = await GetAuthorizedClientAsync();
            var response = await client.PutAsJsonAsync($"api/admin/roles/{id}", role);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Ошибка авторизации при обновлении роли");
                return false;
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в UpdateRoleAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/admin/roles/{id}");
        return response.IsSuccessStatusCode;
    }

}

public class AuthResponse
{
    public string Token { get; set; }
}

public class User
{
    public int UserId { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public int RoleId { get; set; }
}

public class ProductApiResponse
{
    public Catalog Product { get; set; }
    public List<Review> Reviews { get; set; }
}
