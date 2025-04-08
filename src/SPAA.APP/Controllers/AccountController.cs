using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projeto.App.ViewModels;
using SPAA.App.ViewModels;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;

public class AccountController : Controller
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IEmailService _emailService;

    public AccountController(IAlunoRepository alunoRepository,
                             IApplicationUserRepository applicationUserRepository,
                             IEmailService emailService)
    {
        _alunoRepository = alunoRepository;
        _applicationUserRepository = applicationUserRepository;
        _emailService = emailService;

    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (!ModelState.IsValid)
            return View(registerViewModel);

        try
        {
            // Verificação de duplicidade de e-mail
            var userExistente = await _applicationUserRepository.ObterPorEmail(registerViewModel.Email);
            if (userExistente != null)
            {
                ModelState.AddModelError("Email", "Esse e-mail já está registrado.");
                return View(registerViewModel);
            }

            // Verificação de duplicidade de matrícula
            var alunoExistente = await _alunoRepository.ObterPorId(registerViewModel.Matricula);
            if (alunoExistente != null)
            {
                ModelState.AddModelError("Matricula", "Essa matrícula já pertence a outro usuário.");
                return View(registerViewModel);
            }

            // Criação do usuário
            var user = new ApplicationUser
            {
                UserName = registerViewModel.Matricula.ToString(),
                Email = registerViewModel.Email
            };

            var result = await _applicationUserRepository.RegistrarApplicationUser(user, registerViewModel.Senha);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(registerViewModel);
            }

            // Criação do aluno vinculado ao usuário
            var aluno = new Aluno
            {
                Matricula = registerViewModel.Matricula,
                Nome = registerViewModel.Nome,
                SemestreEntrada = $"{registerViewModel.SemestreEntrada}/{registerViewModel.AnoEntrada}",
                UserId = user.Id
            };

            try
            {
                await _alunoRepository.Adicionar(aluno);

                // TempData para passar a mensagem de sucesso
                TempData["MensagemSucesso"] = "Cadastro realizado com sucesso!";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                await _applicationUserRepository.RemoverApplicationUser(user.Id);
                ModelState.AddModelError(string.Empty, $"Erro ao salvar o aluno no banco de dados: {ex.Message}");
                return View(registerViewModel);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Erro durante o cadastro: {ex.Message}");
            return View(registerViewModel);
        }
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

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _applicationUserRepository.ObterPorEmail(model.Email);
        if (user == null)
        {
            // Não revelar se o usuário não existe
            TempData["MensagemSucesso"] = "Se o e-mail estiver cadastrado, você receberá um link de redefinição de senha.";
            return RedirectToAction("Login");
        }

        var token = await _applicationUserRepository.GerarTokenResetSenha(user);

        var callbackUrl = Url.Action(
            nameof(ResetPassword),
            "Account",
            new { token, email = user.Email },
            protocol: HttpContext.Request.Scheme);

        // Enviar e-mail com o link de redefinição (você pode usar um serviço de e-mail)
        await _emailService.EnviarEmailAsync(user.Email, "Redefinição de Senha",
            $"Clique no link para redefinir sua senha: <a href='{callbackUrl}'>Redefinir Senha</a>");

        TempData["MensagemSucesso"] = "Se o e-mail estiver cadastrado, você receberá um link de redefinição de senha.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        var model = new ResetPasswordViewModel { Token = token, Email = email };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _applicationUserRepository.ObterPorEmail(model.Email);
        if (user == null)
        {
            TempData["MensagemErro"] = "Usuário não encontrado.";
            return RedirectToAction("Login");
        }

        var result = await _applicationUserRepository.ResetarSenha(user, model.Token, model.Senha);
        if (result.Succeeded)
        {
            TempData["MensagemSucesso"] = "Senha redefinida com sucesso!";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
}

