// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using WEbAPi.Models;

public class AuthController : Controller
{
    private readonly ApiService _apiService;

    public AuthController(ApiService apiService)
    {
        _apiService = apiService;
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginRequest()); // Используем LoginRequest вместо LoginModel
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var token = await _apiService.LoginAsync(model.Login, model.Password); // Используем Login
        if (token == null)
        {
            ModelState.AddModelError("", "Неверные учетные данные");
            return View(model);
        }

        Response.Cookies.Append("jwt_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.Now.AddHours(1)
        });

        return RedirectToAction("Index", "Home");
    }

}


