using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Основные страницы
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About() => View();
        public IActionResult Privacy() => View();
        public IActionResult BuyBook() => View();
        public IActionResult Favorite() => View();

        // Аутентификация
        [HttpGet]
        public IActionResult Login() => View();



        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var loginRequest = new { model.Login, model.Password };
                var content = new StringContent(JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8, "application/json");

                var response = await _apiService.GetHttpClient().PostAsync("api/AuthApi/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Ошибка входа: {errorContent}");
                    return View(model);
                }

                // Читаем ответ как строку и десериализуем вручную
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.User == null)
                {
                    ModelState.AddModelError("", "Неверный формат ответа от сервера");
                    return View(model);
                }

                // Создаем claims на основе данных из API
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
                    new Claim(ClaimTypes.Name, result.User.Login),
                    new Claim(ClaimTypes.Email, result.User.Email ?? ""),
                    new Claim(ClaimTypes.Role, result.User.Role ?? "user")
                };

                var identity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddHours(1)
                    });

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка входа: {ex.Message}");
                return View(model);
            }
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
            if (!ModelState.IsValid) return View(model);

            try
            {
                var token = await _apiService.RegisterAsync(model.Login, model.Email, model.Password);
                var response = await _apiService.GetHttpClient().GetAsync($"api/AuthApi/user-by-login/{model.Login}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Не удалось получить данные пользователя");
                }

                var user = await response.Content.ReadFromJsonAsync<UserInfo>();
                await SetAuthCookie(user, token);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Index");
        }

        private async Task SetAuthCookie(UserInfo user, string token)
        {
            // Устанавливаем JWT токен в cookie
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(1)
            });

            // Создаем claims с ролью
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "user"),
                new Claim("JWT", token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
        }
    }
}