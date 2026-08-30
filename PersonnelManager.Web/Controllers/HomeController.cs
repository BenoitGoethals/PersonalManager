using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonnelManager.Web.Controllers;

[AllowAnonymous]
public sealed class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Personnel");

    public IActionResult Error() => View();
}
