using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;

namespace ROTA_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["PageTitle"] = "Welcome to ROTA Management"; // More specific title
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var name = User.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value
                           ?? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                           ?? "there";

                ViewData["UserGreeting"] = $"Hello, {name}!";
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
