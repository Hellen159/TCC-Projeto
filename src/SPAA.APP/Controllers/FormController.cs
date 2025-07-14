using Microsoft.AspNetCore.Mvc;
using SPAA.APP.Models; // Seu modelo de usuário (ex: ApplicationUser)
using SPAA.Business.Interfaces.Repository; // Para IAreaInteresseAlunoRepository
using SPAA.Business.Models; // Para AreaInteresseAluno
using SPAA.App.ViewModels; // Para PerfilAlunoSalvarViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Para UserManager
using System;
using System.Linq;
using System.Threading.Tasks; // Para usar métodos assíncronos

namespace SPAA.App.Controllers
{
    [Authorize]
    public class FormController : Controller
    {
        private readonly IAreaInteresseAlunoRepository _areaInteresseAlunoRepository;
        private readonly UserManager<ApplicationUser> _userManager; // Adicione esta linha

        private static readonly IReadOnlyDictionary<string, int> _areaInteresseNameToIdMap = new Dictionary<string, int>
        {
            // Removi os espaços extras que tinham no CalcularPerfil anteriormente.
            // GARANTA que os nomes aqui batem exatamente com o que é retornado pelo CalcularPerfil
            {"Programação e Infraestrutura Técnica", 1},
            {"Projeto e Análise de Software", 2},
            {"Qualidade, Testes e Manutenção", 3},
            {"Gestão de Produtos e Processos", 4},
            {"Fundamentos Matemáticos e Computacionais", 5}
        };

        // Adicione UserManager ao construtor
        public FormController(IAreaInteresseAlunoRepository areaInteresseAlunoRepository, UserManager<ApplicationUser> userManager)
        {
            _areaInteresseAlunoRepository = areaInteresseAlunoRepository;
            _userManager = userManager; // Inicialize
        }


        public IActionResult FormAluno()
        {
            ViewData["LayoutType"] = "formulario";
            return View();
        }

        [HttpPost]
        public IActionResult CalcularPerfil([FromBody] List<RespostasPerguntasFormularioAlunoViewModel> respostas)
        {
            // Seu código existente para CalcularPerfil permanece o mesmo
            if (respostas == null || !respostas.Any())
                return Json(new { error = "Nenhuma resposta recebida." });

            var perfilContagem = new Dictionary<string, int>();
            int totalValidos = 0;

            foreach (var resp in respostas)
            {
                if (resp.Perfil == "Nada") continue;

                if (!perfilContagem.ContainsKey(resp.Perfil))
                    perfilContagem[resp.Perfil] = 0;

                perfilContagem[resp.Perfil]++;
                totalValidos++;
            }

            var perfisPorcentagem = perfilContagem
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (kvp.Value * 100.0) / totalValidos
                );

            var perfisFiltrados = perfisPorcentagem
                .Where(kvp => kvp.Value >= 22) // Apenas perfis com pelo menos 22%
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            string principal = null;
            List<string> secundarios = new List<string>();

            if (perfisFiltrados.Count > 0)
            {
                principal = $"{perfisFiltrados[0].Key}"; // Removido o espaço extra
                if (perfisFiltrados.Count > 1)
                {
                    secundarios.Add($"{perfisFiltrados[1].Key}");
                    if (perfisFiltrados.Count > 2)
                    {
                        secundarios.Add($"{perfisFiltrados[2].Key}");
                    }
                }
            }

            return Json(new
            {
                principal = principal ?? "Nenhum perfil detectado.",
                secundarios = secundarios
            });
        }


        [HttpPost]
        public async Task<IActionResult> SalvarPerfil([FromBody] PerfilAlunoSalvarViewModel model)
        {
            var matricula = User.Identity.Name;

            if (model == null)
            {
                return Json(new { success = false, message = "Dados do perfil inválidos." });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Usuário não autenticado ou ID de usuário não encontrado.", redirectUrl = Url.Action("Login", "Account") });
            }

            try
            {
                int? perfilPrincipalId = null;
                if (!string.IsNullOrEmpty(model.Principal) && _areaInteresseNameToIdMap.TryGetValue(model.Principal.Trim(), out int idPrincipal))
                {
                    perfilPrincipalId = idPrincipal;
                }

                List<int> secundariosIds = new List<int>();
                foreach (var secundarioNome in model.Secundarios)
                {
                    if (_areaInteresseNameToIdMap.TryGetValue(secundarioNome.Trim(), out int idSecundario))
                    {
                        secundariosIds.Add(idSecundario);
                    }
                }
                // Converte a lista de IDs secundários para uma string separada por ';'
                string secundariosIdsString = string.Join(";", secundariosIds);
                // --- FIM DA CONVERSÃO ---


                var areaInteresseAlunoExistente = await _areaInteresseAlunoRepository.AlunoJaTemAreaInteresse(matricula);

                if (areaInteresseAlunoExistente == true)
                {
                    await _areaInteresseAlunoRepository.ExcluirAreaInteresseAluno(matricula);
                }

                var novoAreaInteresseAluno = new AreaInteresseAluno
                {
                    Matricula = matricula,
                    AreaInteressePrincipal = perfilPrincipalId.ToString(),
                    AreaInteresseSecundaria = secundariosIdsString
                };
                await _areaInteresseAlunoRepository.Adicionar(novoAreaInteresseAluno);

                return Json(new { success = true, message = "Seu perfil foi salvo com sucesso!", redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar perfil do usuário {userId}: {ex.Message}");
                return Json(new { success = false, message = "Ocorreu um erro interno ao salvar o perfil." });
            }

        }
    }
}