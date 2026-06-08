using Microsoft.AspNetCore.Mvc;

namespace Keep_on_Rolling.Controllers;

public class ProfileController : Controller
{
    public IActionResult Index() => View();
}
