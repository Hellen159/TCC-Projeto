using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;

namespace SPAA.App.Controllers
{
    public class DashBoardController : Controller
    {
        public IActionResult Index()
        {
            var turmaViewModel = new TurmaListaViewModel
            {
                Titulo = "Turmas Disponiveis",
                Turmas = new List<TurmaViewModel>
        {
            new TurmaViewModel
            {
                CodigoDisciplina = "INF101",
                NomeDisciplina = "Algoritmos",
                NomeProfessor = "João",
                Horario = "Seg 10h"
            }
        }
            };

            return View(turmaViewModel);
        }
    }
}
