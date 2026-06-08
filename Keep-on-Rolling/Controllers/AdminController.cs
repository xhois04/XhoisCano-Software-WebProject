using Microsoft.AspNetCore.Mvc;

namespace Keep_on_Rolling.Controllers;

public class AdminController : Controller
{
    public IActionResult Index() => View();
}