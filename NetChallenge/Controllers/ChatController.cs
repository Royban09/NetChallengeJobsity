using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetChallenge.Models;

[Authorize]
public class ChatController : Controller
{
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Index()
    {
        return View();
    }

}
