using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEbAPi.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApiService _apiService;

    public AdminController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Users()
    {
        var users = await _apiService.GetUsersAsync();
        return View(users);
    }

    public async Task<IActionResult> Catalogs()
    {
        var catalogs = await _apiService.GetCatalogsAsync();
        return View(catalogs);
    }
}