using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEbAPi.Models;

[Authorize(Roles = "2")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly ApiService _apiService;

    public AdminController(ApiService apiService)
    {
        _apiService = apiService;
    }

    #region Catalogs Views

    [HttpGet("Catalogs")]
    public async Task<IActionResult> Catalogs()
    {
        var catalogs = await _apiService.GetCatalogsAsync();
        return View("CatalogsIndex", catalogs);
    }

    [HttpGet("Catalogs/Create")]
    public IActionResult CatalogsCreate()
    {
        return View("CatalogsCreate");
    }

    [HttpPost("Catalogs/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CatalogsCreate(Catalog catalog)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return View(catalog);
            }

            var result = await _apiService.CreateCatalogAsync(catalog);
            if (result)
            {
                TempData["Success"] = "Каталог успешно добавлен";
                return RedirectToAction(nameof(Catalogs));
            }

            ModelState.AddModelError("", "Ошибка при добавлении каталога");
            return View(catalog);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            ModelState.AddModelError("", $"Ошибка: {ex.Message}");
            return View(catalog);
        }
    }

    [HttpGet("Catalogs/Edit/{id}")]
    public async Task<IActionResult> CatalogsEdit(int id)
    {
        var catalog = await _apiService.GetCatalogByIdAsync(id);
        if (catalog == null) return NotFound();
        return View("CatalogsEdit", catalog);
    }

    [HttpPost("Catalogs/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CatalogsEdit(int id, Catalog catalog)
    {
        if (id != catalog.CatalogsId)
            return NotFound();

        if (!ModelState.IsValid)
            return View("CatalogsEdit", catalog);

        try
        {
            if (await _apiService.UpdateCatalogAsync(id, catalog))
                return RedirectToAction(nameof(Catalogs));

            ModelState.AddModelError("", "Failed to update catalog");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error: {ex.Message}");
        }

        return View("CatalogsEdit", catalog);
    }

    [HttpGet("Catalogs/Delete/{id}")]
    public async Task<IActionResult> CatalogsDelete(int id)
    {
        var catalog = await _apiService.GetCatalogByIdAsync(id);
        if (catalog == null) return NotFound();
        return View("CatalogsDelete", catalog);
    }

    [HttpPost("Catalogs/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CatalogsDeleteConfirmed(int id)
    {
        try
        {
            if (await _apiService.DeleteCatalogAsync(id))
                return RedirectToAction(nameof(Catalogs));

            TempData["Error"] = "Failed to delete catalog";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(CatalogsDelete), new { id });
    }
    #endregion

    #region Categories Views

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _apiService.GetCategoriesAsync();
        return View("CategoriesIndex", categories);
    }

    [HttpGet("Categories/Create")]
    public IActionResult CategoriesCreate()
    {
        return View("CategoriesCreate");
    }

    [HttpPost("Categories/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriesCreate(Category category)
    {
        if (ModelState.IsValid)
        {
            if (await _apiService.CreateCategoryAsync(category))
                return RedirectToAction(nameof(Categories));
            ModelState.AddModelError("", "Ошибка при создании категории");
        }
        return View("CategoriesCreate", category);
    }

    [HttpGet("Categories/Edit/{id}")]
    public async Task<IActionResult> CategoriesEdit(int id)
    {
        var category = await _apiService.GetCategoryAsync(id);
        if (category == null) return NotFound();
        return View("CategoriesEdit", category);
    }

    [HttpPost("Categories/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriesEdit(int id, Category category)
    {
        if (id != category.CategoryId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _apiService.UpdateCategoryAsync(id, category))
                return RedirectToAction(nameof(Categories));
            ModelState.AddModelError("", "Ошибка при обновлении категории");
        }
        return View("CategoriesEdit", category);
    }

    [HttpGet("Categories/Delete/{id}")]
    public async Task<IActionResult> CategoriesDelete(int id)
    {
        var category = await _apiService.GetCategoryAsync(id);
        if (category == null) return NotFound();
        return View("CategoriesDelete", category);
    }

    [HttpPost("Categories/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriesDeleteConfirmed(int id)
    {
        if (await _apiService.DeleteCategoryAsync(id))
            return RedirectToAction(nameof(Categories));
        return RedirectToAction("CategoriesDelete", new { id, error = true });
    }

    #endregion

    #region Users Views

    [HttpGet("Users")]
    public async Task<IActionResult> Users()
    {
        List<User> users = await _apiService.GetUsersAsync();
        return View("UsersIndex", users); 
    }

    [HttpGet("Users/Create")]
    public async Task<IActionResult> UsersCreate()
    {
        ViewBag.Roles = await _apiService.GetRolesAsync();
        return View("UsersCreate");
    }

    [HttpPost("Users/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsersCreate(User user)
    {
        if (ModelState.IsValid)
        {
            if (await _apiService.CreateUserAsync(user))
                return RedirectToAction(nameof(Users));
            ModelState.AddModelError("", "Ошибка при создании пользователя");
        }
        ViewBag.Roles = await _apiService.GetRolesAsync();
        return View("UsersCreate", user);
    }

    [HttpGet("Users/Edit/{id}")]
    public async Task<IActionResult> UsersEdit(int id)
    {
        var user = await _apiService.GetUserAsync(id);
        if (user == null) return NotFound();
        ViewBag.Roles = await _apiService.GetRolesAsync();
        return View("UsersEdit", user);
    }

    [HttpPost("Users/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsersEdit(int id, User user)
    {
        if (id != user.UserId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _apiService.UpdateUserAsync(id, user))
                return RedirectToAction(nameof(Users));
            ModelState.AddModelError("", "Ошибка при обновлении пользователя");
        }
        ViewBag.Roles = await _apiService.GetRolesAsync();
        return View("UsersEdit", user);
    }

    [HttpGet("Users/Delete/{id}")]
    public async Task<IActionResult> UsersDelete(int id)
    {
        var user = await _apiService.GetUserAsync(id);
        if (user == null) return NotFound();
        return View("UsersDelete", user);
    }

    [HttpPost("Users/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsersDeleteConfirmed(int id)
    {
        if (await _apiService.DeleteUserAsync(id))
            return RedirectToAction(nameof(Users));
        return RedirectToAction("UsersDelete", new { id, error = true });
    }

    #endregion

    #region Orders Views

    [HttpGet("Orders")]
    public async Task<IActionResult> Orders()
    {
        var orders = await _apiService.GetOrdersAsync();
        return View("OrdersIndex", orders);
    }

    [HttpGet("Orders/Edit/{id}")]
    public async Task<IActionResult> OrdersEdit(int id)
    {
        var order = await _apiService.GetOrderAsync(id);
        if (order == null) return NotFound();
        return View("OrdersEdit", order);
    }

    [HttpPost("Orders/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrdersEdit(int id, Order order)
    {
        if (id != order.OrdersId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _apiService.UpdateOrderAsync(id, order))
                return RedirectToAction(nameof(Orders));
            ModelState.AddModelError("", "Ошибка при обновлении заказа");
        }
        return View("OrdersEdit", order);
    }

    [HttpGet("Orders/Details/{id}")]
    public async Task<IActionResult> OrdersDetails(int id)
    {
        var order = await _apiService.GetOrderAsync(id);
        if (order == null) return NotFound();
        return View("OrdersDetails", order);
    }

    [HttpGet("Orders/Delete/{id}")]
    public async Task<IActionResult> OrdersDelete(int id)
    {
        try
        {
            var order = await _apiService.GetOrderAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Заказ не найден";
                return RedirectToAction(nameof(Orders));
            }

            return View("OrdersDelete", order);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при получении заказа: {ex.Message}";
            return RedirectToAction(nameof(Orders));
        }
    }

    [HttpPost("Orders/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrdersDeleteConfirmed(int id)
    {
        try
        {
            var result = await _apiService.DeleteOrderAsync(id);
            if (result)
            {
                TempData["Success"] = "Заказ успешно удалён";
                return RedirectToAction(nameof(Orders));
            }

            TempData["Error"] = "Не удалось удалить заказ";
            return RedirectToAction(nameof(OrdersDelete), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при удалении заказа: {ex.Message}";
            return RedirectToAction(nameof(OrdersDelete), new { id });
        }
    }
    #endregion

    #region PosOrders Views

    [HttpGet("PosOrders")]
    public async Task<IActionResult> PosOrders()
    {
        var posOrders = await _apiService.GetPosOrdersAsync();
        return View("PosOrdersIndex", posOrders);
    }

    [HttpGet("PosOrders/Create")]
    public IActionResult PosOrdersCreate()
    {
        return View("PosOrdersCreate");
    }

    [HttpPost("PosOrders/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PosOrdersCreate(PosOrder posOrder)
    {
        if (ModelState.IsValid)
        {
            if (await _apiService.CreatePosOrderAsync(posOrder))
                return RedirectToAction(nameof(PosOrders));
            ModelState.AddModelError("", "Ошибка при создании позиции заказа");
        }
        return View("PosOrdersCreate", posOrder);
    }

    [HttpGet("PosOrders/Edit/{id}")]
    public async Task<IActionResult> PosOrdersEdit(int id)
    {
        var posOrder = await _apiService.GetPosOrderAsync(id);
        if (posOrder == null) return NotFound();
        return View("PosOrdersEdit", posOrder);
    }

    [HttpPost("PosOrders/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PosOrdersEdit(int id, PosOrder posOrder)
    {
        if (id != posOrder.PosOrderId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _apiService.UpdatePosOrderAsync(id, posOrder))
                return RedirectToAction(nameof(PosOrders));
            ModelState.AddModelError("", "Ошибка при обновлении позиции заказа");
        }
        return View("PosOrdersEdit", posOrder);
    }

    [HttpGet("PosOrders/Delete/{id}")]
    public async Task<IActionResult> PosOrdersDelete(int id)
    {
        var posOrder = await _apiService.GetPosOrderAsync(id);
        if (posOrder == null) return NotFound();
        return View("PosOrdersDelete", posOrder);
    }

    [HttpPost("PosOrders/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PosOrdersDeleteConfirmed(int id)
    {
        if (await _apiService.DeletePosOrderAsync(id))
            return RedirectToAction(nameof(PosOrders));
        return RedirectToAction("PosOrdersDelete", new { id, error = true });
    }

    #endregion

    #region Roles Views

    [HttpGet("Roles")]
    public async Task<IActionResult> Roles()
    {
        var roles = await _apiService.GetRolesAsync();
        return View("RolesIndex", roles);
    }

    [HttpGet("Roles/Edit/{id}")]
    public async Task<IActionResult> RolesEdit(int id)
    {
        try
        {
            var role = await _apiService.GetRoleAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Роль не найдена";
                return RedirectToAction(nameof(Roles));
            }
            return View("RolesEdit", role);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("RolesEdit", new { id }) });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при загрузке роли: {ex.Message}";
            return RedirectToAction(nameof(Roles));
        }
    }

    [HttpPost("Roles/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolesEdit(int id, Role role)
    {
        if (id != role.RoleId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _apiService.UpdateRoleAsync(id, role))
                return RedirectToAction(nameof(Roles));
            ModelState.AddModelError("", "Ошибка при обновлении роли");
        }
        return View("RolesEdit", role);
    }

    [HttpGet("Roles/Delete/{id}")]
    public async Task<IActionResult> RolesDelete(int id)
    {
        var role = await _apiService.GetRoleAsync(id);
        if (role == null) return NotFound();
        return View("RolesDelete", role);
    }

    [HttpPost("Roles/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolesDeleteConfirmed(int id)
    {
        if (await _apiService.DeleteRoleAsync(id))
            return RedirectToAction(nameof(Roles));
        return RedirectToAction("RolesDelete", new { id, error = true });
    }

    #endregion
}