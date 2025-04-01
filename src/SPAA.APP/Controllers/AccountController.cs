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

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (ModelState.IsValid)
        {
            //var existingUser = await _applicationUserRepository.FindByEmailAsync(registerViewModel.Email);
            //if (existingUser != null)
            //{
            //    registerViewModelState.AddregisterViewModelError("Email", "Este email já está em uso.");
            //    return View(registerViewModel);
            //}

            // 1. Criar o ApplicationUser
            var user = new ApplicationUser
            {
                UserName = registerViewModel.Matricula,
                Email = registerViewModel.Email
            };

            var result = await _applicationUserRepository.RegistrarApplicationUser(user, registerViewModel.Senha); // Chamar serviço para criar o usuário

            if (result.Succeeded)
            {
                // 2. Criar o Aluno
                var aluno = new Aluno
                {
                    Matricula = registerViewModel.Matricula,
                    Nome = registerViewModel.Nome,
                    SemestreEntrada = registerViewModel.SemestreEntrada,
                    UserId = user.Id // Relacionamento entre o Aluno e o ApplicationUser
                };

                // 3. Salvar o Aluno no banco de dados
                await _alunoRepository.CriarAluno(aluno); // Chamar serviço para criar o aluno

                // Redireciona ou faz algo após o sucesso
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Adiciona erros ao registerViewModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

        }

        return View(registerViewModel); // Se o modelo não for válido, retorna a view com os erros.
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        var result = await _applicationUserRepository.LogarApplicationUser(loginViewModel.Matricula, loginViewModel.Senha);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Login inválido");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _applicationUserRepository.LogoutApplicationUser();
        return RedirectToAction("Login", "Account");
    }
}

