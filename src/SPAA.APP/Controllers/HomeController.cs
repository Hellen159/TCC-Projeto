using Microsoft.AspNetCore.Mvc;
using SPAA.APP.Models;
using System.Diagnostics;

namespace SPAA.APP.Controllers
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
            // Verifica se o usu�rio est� autenticado
            if (!User.Identity.IsAuthenticated)
            {
                // Redireciona para a p�gina de login se n�o autenticado
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
