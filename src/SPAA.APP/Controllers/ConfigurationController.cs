using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;

namespace SPAA.App.Controllers
{
    [Authorize]
    public class ConfigurationController : Controller
    {
        private readonly IAlunoRepository _alunoRepository;
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfigurationController(IAlunoRepository alunoRepository,
                                        UserManager<ApplicationUser> userManager,
                                        IApplicationUserRepository applicationUserRepository,
                                        IAlunoDisciplinaRepository alunoDisciplinaRepository)
        {
            _alunoRepository = alunoRepository;
            _userManager = userManager;
            _applicationUserRepository = applicationUserRepository;
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
        }

        [HttpGet]
        public IActionResult Configurations()
        {
            return View();
        }

        // POST: Alterar Nome
        [HttpPost]
        public async Task<IActionResult> AlterarNome(ConfigurationViewModel model)
        {
            var result = await _alunoRepository.AlterarNome(User.Identity.Name, model.NovoNome);

            if (result)
            {
                TempData["MensagemSucesso"] = "Nome alterado com sucesso!";
                return RedirectToAction("Configurations", "Configuration");
            }

            TempData["ErrorMessage"] = "Erro ao alterar nome!";
            return RedirectToAction( "Configurations", "Configuration");
        }

        //// POST: Refazer Formulário
        //[HttpPost]
        //public IActionResult RefazerFormulario()
        //{
        //    // Lógica para refazer o formulário
        //    TempData["Mensagem"] = "Formulário refeito com sucesso!";
        //    return RedirectToAction("Index");
        //}

        // POST: Alterar Senha
        [HttpPost]
        public async Task<IActionResult> AlterarSenha(ConfigurationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            if (!await _applicationUserRepository.VerificarSenhaAtual(user, model.SenhaAtual))
            {
                TempData["ErrorMessage"] = "Senha atual incorreta.";
                return RedirectToAction("Configurations", "Configuration");
            }

            var resultado = await _applicationUserRepository.AlterarSenha(user, model.SenhaAtual, model.NovaSenha);
            if (resultado.Succeeded)
            {
                TempData["MensagemSucesso"] = "Senha alterada com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", resultado.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Configurations", "Configuration");
        }

        [HttpGet]
        public IActionResult ConfirmarExclusao()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConta(ConfirmarExclusaoViewModel model)
        {
            if (model.Matricula == null)
            {
                TempData["ErrorMessage"] = "A matrícula é obrigatória para confirmar a exclusão.";
                return RedirectToAction("ConfirmarExclusao");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.UserName != model.Matricula)
            {
                TempData["ErrorMessage"] = "A matrícula informada não corresponde ao usuário.";
                return RedirectToAction("ConfirmarExclusao");
            }

            var deleteAluno = await _alunoRepository.Remover(user.UserName);
            var deleteAlunoDisciplina = await _alunoDisciplinaRepository.ExcluirDisciplinasDoAluno(user.UserName);
            var result = await _applicationUserRepository.RemoverApplicationUser(user.Id);

            if (result.Succeeded && deleteAluno && deleteAlunoDisciplina && deleteAlunoDisciplina)
            {
                await _applicationUserRepository.LogoutApplicationUser();
                TempData["MensagemSucesso"] = "Conta excluída com sucesso.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao excluir conta.";
                return RedirectToAction("ConfirmarExclusao", "Configuration");
            }
        }
    }
}

