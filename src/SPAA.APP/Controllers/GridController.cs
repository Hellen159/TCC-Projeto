using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;

namespace SPAA.App.Controllers
{
    public class GridController : Controller
    {
        [HttpGet]
        public IActionResult MontarGrade()
        {
            return View("~/Views/DashBoard/MontarGrade.cshtml");
        }

        [HttpPost]
        public IActionResult MontarGrade(MontarGradeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Processar os dados recebidos
                foreach (var horario in model.HorariosMarcados)
                {
                    // Exemplo: "6M2"
                    // Fazer as paradas depois
                }

                ViewBag.Mensagem = "Dados recebidos com sucesso!";
            }

            return View("~/Views/DashBoard/MontarGrade.cshtml");
        }
    }
}