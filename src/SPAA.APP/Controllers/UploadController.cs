using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPAA.Business.Interfaces;
using SPAA.Data.Repository;

namespace SPAA.App.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        private readonly IAlunoRepository _alunoRepository;

        public UploadController(IAlunoDisciplinaRepository alunoDisciplinaRepository, 
                                IAlunoRepository alunoRepository)
        {
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
            _alunoRepository = alunoRepository;
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
        public async Task<IActionResult> UploadHistorico(IFormFile historico, string returnAction = null, string returnController = null)
        {
            //var alunoJaAnexouHistorico = await _alunoRepository.AlunoJaAnexouHistorico(User.Identity.Name);
            var existeDadosAlunoEmAlunosDisciplinas = await _alunoDisciplinaRepository.ExcluirDisciplinasDoAluno(User.Identity.Name);
            //if (alunoJaAnexouHistorico)
            //{
            //    await _alunoDisciplinaRepository.ExcluirDisciplinasDoAluno(User.Identity.Name);
            //}

            var result = await _alunoDisciplinaRepository.ConsumirHistoricoPdf(historico, User.Identity.Name);

            if (!result.isValid)
            {
                TempData["ErrorMessage"] = result.mensagem;
                return RedirectToAction(returnAction ?? "UploadHistorico", returnController ?? "Upload");
            }

            TempData["MensagemSucesso"] = result.mensagem;
            return RedirectToAction(returnAction ?? "Index", returnController ?? "Home");
        }
    }
}
