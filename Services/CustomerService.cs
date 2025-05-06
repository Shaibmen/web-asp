using WEbAPi.Models.Dto;
using WEbAPi.Models;

public class CustomerService
{
    private readonly ApiClientService _apiClient;

    public CustomerService(ApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<Catalog>> GetCatalogAsync(string? category, string? searchQuery, string? sortBy)
    {
        var url = $"api/CustomerApi/catalog?category={category}&searchQuery={searchQuery}&sortBy={sortBy}";
        var result = await _apiClient.GetAsync<List<Catalog>>(url);
        return result ?? new List<Catalog>();
    }

    public async Task<(List<PosOrder> items, decimal totalSum)> GetCartAsync()
    {
        var result = await _apiClient.GetAsync<CartResponse>("api/CustomerApi/cart");
        return (result?.Items ?? new List<PosOrder>(), result?.TotalSum ?? 0);
    }

    public async Task<string?> AddToCartAsync(AddToCartDto model)
    {
        return await _apiClient.PostAsync("api/CustomerApi/add-to-cart", model);
    }

    public async Task<string?> UpdateCartItemAsync(int posOrderId, int newCount)
    {
        return await _apiClient.PostAsync("api/CustomerApi/update-cart-item",
            new { posOrderId, newCount });
    }

    public async Task<string?> AddReviewAsync(ReviewFormModel model)
    {
        return await _apiClient.PostAsync("api/CustomerApi/add-review", model);
    }
}