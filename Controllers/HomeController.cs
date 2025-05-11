using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WEbAPi.Models;

namespace WEbAPi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public HomeController(ApiService apiService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _apiService = apiService;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About() => View();
        public IActionResult Privacy() => View();
        public IActionResult BuyBook() => View();
        public IActionResult Favorite() => View();

        [HttpGet]
        public IActionResult Login() => View();


        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var token = await _apiService.LoginAsync(model.Login, model.Password);
                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError("", "Invalid login or password");
                    return View(model);
                }

                SetJwtCookie(token);

                var user = await _apiService.GetUserAsync(model.Login);
                if (user == null)
                {
                    ModelState.AddModelError("", "Failed to get user data");
                    return View(model);
                }

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Login error: {ex.Message}");
                return View(model);
            }
        }



        private void SetJwtCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(1),
            };

            Response.Cookies.Append("jwt_token", token, cookieOptions);
        }


        public class LoginResult
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("user")]
            public UserInfo User { get; set; }
        }

        public class UserInfo
        {
            [JsonPropertyName("userId")]
            public int UserId { get; set; }

            [JsonPropertyName("login")]
            public string Login { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("role")]
            public string Role { get; set; }
        }

        [HttpGet]
        public IActionResult Register() => View();
        [HttpPost]
public async Task<IActionResult> Register(RegisterRequest model)
{
    if (!ModelState.IsValid)
    {
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"Validation error: {error.ErrorMessage}");
        }
        return View(model);
    }

    try
    {
        var requestData = new
        {
            login = model.Login,
            email = model.Email,
            password = model.Password
        };

        Console.WriteLine($"Sending: {JsonSerializer.Serialize(requestData)}");
        
        var response = await _httpClient.PostAsJsonAsync("api/AuthApi/register", requestData);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"Received: {response.StatusCode}, {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", $"Ошибка сервера: {responseContent}");
            return View(model);
        }

        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
        
        if (string.IsNullOrEmpty(authResponse?.Token))
        {
            ModelState.AddModelError("", "Не удалось получить токен");
            return View(model);
        }

        Response.Cookies.Append("jwt_token", authResponse.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.Now.AddHours(1)
        });

        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex}");
        ModelState.AddModelError("", $"Ошибка соединения: {ex.Message}");
        return View(model);
    }
}
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Index");
        }

        

       
    }
}