using Newtonsoft.Json;
using System.Net.Http.Headers;

public class ApiClientService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://localhost:7111/");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // ❌ Больше не устанавливаем Authorization заголовок
        return client;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var client = CreateClient();
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return default;

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task<string?> PostAsync<T>(string url, T data)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(url, data);
        return await response.Content.ReadAsStringAsync();
    }
}
