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
        if (!ModelState.IsValid)
        {
            return View(loginViewModel);
        }

        var result = await _applicationUserRepository.LogarApplicationUser(loginViewModel.Matricula, loginViewModel.Senha);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        TempData["MensagemErro"] = "Usuário ou senha incorretos.";
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

        }

        return View(model); // Se o modelo não for válido, retorna a view com os erros.
    }
}
