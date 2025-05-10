using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using WEbAPi.Models;

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
            var response = await client.GetAsync($"https://localhost:7111/api/Customer/average-rating/{productId}");

            double averageRating = 0;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var ratingResponse = JsonConvert.DeserializeObject<AverageRatingResponse>(json);
                averageRating = ratingResponse?.AverageRating ?? 0;
            }

            return View(averageRating);
        }

    }
}


public class AverageRatingResponse
{
    public double AverageRating { get; set; }
}
