using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEbAPi.Models;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly ApiService _apiService;

    public AdminController(ApiService apiService)
    {
        _apiService = apiService;
    }

    #region Catalogs Views

    public async Task<IActionResult> Catalogs()
    {
        var catalogs = await _apiService.GetCatalogsAsync();
        return View("CatalogsIndex", catalogs);
    }

    public IActionResult CreateCatalog()
    {
        return View("CatalogsCreate");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCatalog(Catalog catalog)
    {
        if (ModelState.IsValid)
        {
            if (await _apiService.CreateCatalogAsync(catalog))
                return RedirectToAction(nameof(Catalogs));

            ModelState.AddModelError("", "Ошибка при создании каталога");
        }
        return View("CatalogsCreate", catalog);
    }

    public async Task<IActionResult> EditCatalog(int id)
    {
        var catalog = await _apiService.GetCatalogByIdAsync(id);
        if (catalog == null) return NotFound();
        return View("CatalogsEdit", catalog);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCatalog(int id, Catalog catalog)
    {
        if (id != catalog.CatalogsId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _apiService.UpdateCatalogAsync(id, catalog))
                return RedirectToAction(nameof(Catalogs));

            ModelState.AddModelError("", "Ошибка при обновлении каталога");
        }
        return View("CatalogsEdit", catalog);
    }

    public async Task<IActionResult> DeleteCatalog(int id)
    {
        var catalog = await _apiService.GetCatalogByIdAsync(id);
        if (catalog == null) return NotFound();
        return View("CatalogsDelete", catalog);
    }

    [HttpPost, ActionName("DeleteCatalog")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCatalogConfirmed(int id)
    {
        if (await _apiService.DeleteCatalogAsync(id))
            return RedirectToAction(nameof(Catalogs));

        return RedirectToAction(nameof(DeleteCatalog), new { id, error = true });
    }

    #endregion

    #region Categories Views

    public async Task<IActionResult> Categories()
    {
        var categories = await _apiService.GetCategoriesAsync();
        return View("CategoriesIndex", categories);
    }

    public IActionResult CreateCategory()
    {
        return View("CategoriesCreate");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (ModelState.IsValid)
        {
            if (await _apiService.CreateCategoryAsync(category))
                return RedirectToAction(nameof(Categories));

            ModelState.AddModelError("", "Ошибка при создании категории");
        }
        return View("CategoriesCreate", category);
    }

    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _apiService.GetCategoryAsync(id);
        if (category == null) return NotFound();
        return View("CategoriesEdit", category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, Category category)
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

    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _apiService.GetCategoryAsync(id);
        if (category == null) return NotFound();
        return View("CategoriesDelete", category);
    }

    [HttpPost, ActionName("DeleteCategory")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirmed(int id)
    {
        if (await _apiService.DeleteCategoryAsync(id))
            return RedirectToAction(nameof(Categories));

        return RedirectToAction(nameof(DeleteCategory), new { id, error = true });
    }

    #endregion

    #region Users Views

    public async Task<IActionResult> Users()
    {
        var users = await _apiService.GetUsersAsync();
        return View("UsersIndex", users);
    }

    public async Task<IActionResult> CreateUser()
    {
        ViewBag.Roles = await _apiService.GetRolesAsync();
        return View("UsersCreate");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(User user)
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

    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _apiService.GetUserAsync(id);
        if (user == null) return NotFound();

        ViewBag.Roles = await _apiService.GetRolesAsync();
        return View("UsersEdit", user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, User user)
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

    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _apiService.GetUserAsync(id);
        if (user == null) return NotFound();
        return View("UsersDelete", user);
    }

    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int id)
    {
        if (await _apiService.DeleteUserAsync(id))
            return RedirectToAction(nameof(Users));

        return RedirectToAction(nameof(DeleteUser), new { id, error = true });
    }

    #endregion

    #region Orders Views

    [HttpPost, ActionName("GetOrder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Orders()
    {
        var orders = await _apiService.GetOrdersAsync();
        return View("OrdersIndex", orders);
    }

    public async Task<IActionResult> OrderDetails(int id)
    {
        var order = await _apiService.GetOrderAsync(id);
        if (order == null) return NotFound();
        return View("OrdersDetails", order);
    }

    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _apiService.GetOrderAsync(id);
        if (order == null) return NotFound();
        return View("OrdersDelete", order);
    }

    [HttpPost, ActionName("DeleteOrder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOrderConfirmed(int id)
    {
        if (await _apiService.DeleteOrderAsync(id))
            return RedirectToAction(nameof(Orders));

        return RedirectToAction(nameof(DeleteOrder), new { id, error = true });
    }

    #endregion

    #region PosOrders Views

    public async Task<IActionResult> PosOrders()
    {
        var posOrders = await _apiService.GetPosOrdersAsync();
        return View("PosOrdersIndex", posOrders);
    }

    public IActionResult CreatePosOrder()
    {
        return View("PosOrdersCreate");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePosOrder(PosOrder posOrder)
    {
        if (ModelState.IsValid)
        {
            if (await _apiService.CreatePosOrderAsync(posOrder))
                return RedirectToAction(nameof(PosOrders));

            ModelState.AddModelError("", "Ошибка при создании позиции заказа");
        }
        return View("PosOrdersCreate", posOrder);
    }

    public async Task<IActionResult> EditPosOrder(int id)
    {
        var posOrder = await _apiService.GetPosOrderAsync(id);
        if (posOrder == null) return NotFound();
        return View("PosOrdersEdit", posOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPosOrder(int id, PosOrder posOrder)
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

    public async Task<IActionResult> DeletePosOrder(int id)
    {
        var posOrder = await _apiService.GetPosOrderAsync(id);
        if (posOrder == null) return NotFound();
        return View("PosOrdersDelete", posOrder);
    }


    [HttpPost, ActionName("DeletePosOrder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePosOrderConfirmed(int id)
    {
        if (await _apiService.DeletePosOrderAsync(id))
            return RedirectToAction(nameof(PosOrders));

        return RedirectToAction(nameof(DeletePosOrder), new { id, error = true });
    }

    #endregion

    #region Roles Views

    public async Task<IActionResult> Roles()
    {
        var roles = await _apiService.GetRolesAsync();
        return View("RolesIndex", roles);
    }

    #endregion
}