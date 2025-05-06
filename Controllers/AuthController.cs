using Microsoft.AspNetCore.Mvc;
using WEbAPi.Models;

public class AuthController : Controller
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
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

        var error = await _authService.LoginAsync(model);

        if (error == null)
            return RedirectToAction("Catalog", "Customer");

        ModelState.AddModelError("", error);
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Login");
    }
}
