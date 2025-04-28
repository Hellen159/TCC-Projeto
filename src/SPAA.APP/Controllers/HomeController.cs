using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPAA.APP.Models;
using SPAA.Business.Interfaces;
using SPAA.Data.Context;
using System.Diagnostics;

namespace SPAA.APP.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAlunoRepository _alunoRepository;


        public HomeController(ILogger<HomeController> logger,
                               IAlunoRepository alunoRepository)
        {
            _logger = logger;
            _alunoRepository = alunoRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var alunoJaAnexouHistorico = await _alunoRepository.AlunoJaAnexouHistorico(User.Identity.Name);
            if (alunoJaAnexouHistorico == false)
            {
                return RedirectToAction("UploadHistorico", "Upload"); 
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
