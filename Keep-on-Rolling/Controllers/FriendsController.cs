using Microsoft.AspNetCore.Mvc;

namespace Keep_on_Rolling.Controllers;

public class FriendsController : Controller
{
    public IActionResult Index() => View();
}