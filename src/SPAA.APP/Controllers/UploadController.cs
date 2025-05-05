using Microsoft.AspNetCore.Mvc;
using SPAA.Business.Interfaces;

namespace SPAA.App.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;

        public UploadController(IAlunoDisciplinaRepository alunoDisciplinaRepository)
        {
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
        }

        public IActionResult UploadHistorico()
        {
            return View();
        }

        public IActionResult UploadForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadHistorico(IFormFile historico)
        {
            var result = await _alunoDisciplinaRepository.ConsumirHistoricoPdf(historico, User.Identity.Name);

            if (!result.isValid)
            {
                TempData["ErrorMessage"] = result.mensagem;
                return View();
            }

            TempData["MensagemSucesso"] = result.mensagem;  // Aqui passa a mensagem de sucesso também
            return RedirectToAction("UploadHistorico");
        }
    }
}
