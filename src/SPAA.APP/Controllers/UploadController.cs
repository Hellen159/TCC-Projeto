using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Data.Repository;

namespace SPAA.App.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        private readonly IAlunoRepository _alunoRepository;
        private readonly IAlunoDisciplinaService _alunoDisciplinaService;

        public UploadController(IAlunoDisciplinaRepository alunoDisciplinaRepository, 
                                IAlunoRepository alunoRepository,
                                IAlunoDisciplinaService alunoDisciplinaService)
        {
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
            _alunoRepository = alunoRepository;
            _alunoDisciplinaService = alunoDisciplinaService;
        }

        public IActionResult UploadHistorico()
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

            var result = await _alunoDisciplinaService.ConsumirHistoricoPdf(historico, User.Identity.Name);

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
