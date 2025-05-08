using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace WEbAPi.Components
{
    public class AverageRatingViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AverageRatingViewComponent(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Убедимся, что URL абсолютный
            var response = await client.GetAsync($"https://localhost:7111/api/CustomerApi/average-rating/{productId}");

            double averageRating = 0;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                averageRating = JsonConvert.DeserializeObject<double>(json);
            }

            return View(averageRating);
        }

    }
}
