using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = await _apiService.LoginAsync(model.Login, model.Password);

            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }

            // Аутентификация по логину
            await SetAuthCookie(model.Login, token);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var token = await _apiService.RegisterAsync(
                    model.Login,
                    model.Email, // может быть null
                    model.Password);

                await SetAuthCookie(model.Login, token);
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

        private async Task SetAuthCookie(string login, string token)
        {
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(1)
            });

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, login), // Используем Name вместо Email
        new Claim("JWT", token)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
        }
    }
}