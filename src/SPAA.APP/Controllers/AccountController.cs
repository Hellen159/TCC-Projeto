using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projeto.App.ViewModels;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;

public class AccountController : Controller
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;

    public AccountController(IAlunoRepository alunoRepository,
                             IApplicationUserRepository applicationUserRepository)
    {
        _alunoRepository = alunoRepository;
        _applicationUserRepository = applicationUserRepository;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
        

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            //var existingUser = await _applicationUserRepository.FindByEmailAsync(model.Email);
            //if (existingUser != null)
            //{
            //    ModelState.AddModelError("Email", "Este email já está em uso.");
            //    return View(model);
            //}

            // 1. Criar o ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _applicationUserRepository.CriarApplicationUser(user, model.Senha); // Chamar serviço para criar o usuário

            if (result.Succeeded)
            {
                // 2. Criar o Aluno
                var aluno = new Aluno
                {
                    Matricula = model.Matricula,
                    Nome = model.Nome,
                    SemestreEntrada = model.SemestreEntrada,
                    UserId = user.Id // Relacionamento entre o Aluno e o ApplicationUser
                };

                // 3. Salvar o Aluno no banco de dados
                await _alunoRepository.CriarAluno(aluno); // Chamar serviço para criar o aluno

                // Redireciona ou faz algo após o sucesso
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Adiciona erros ao ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

        }

        return View(model); // Se o modelo não for válido, retorna a view com os erros.
    }
}
