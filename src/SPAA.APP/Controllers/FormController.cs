using Microsoft.AspNetCore.Mvc;
using SPAA.APP.Models;
using SPAA.Business.Interfaces;
using SPAA.Data.Context;
using System.Diagnostics;
using Projeto.App.ViewModels;
using SPAA.App.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace SPAA.App.Controllers
{
    [Authorize]
    public class FormController : Controller
    {
        public IActionResult FormAluno()
        {
            ViewData["LayoutType"] = "formulario";
            return View();
        }


        [HttpPost]
        public IActionResult CalcularPerfil([FromBody] List<RespostasPerguntasFormularioAlunoViewModel> respostas)
        {
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

            // Calcular porcentagens
            var perfisPorcentagem = perfilContagem
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (kvp.Value * 100.0) / totalValidos
                );

            // Filtrar e ordenar por porcentagem
            var perfisFiltrados = perfisPorcentagem
                .Where(kvp => kvp.Value >= 22) // Apenas perfis com pelo menos 35%
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            string principal = null;
            List<string> secundarios = new List<string>();

            if (perfisFiltrados.Count > 0)
            {
                principal = $"{perfisFiltrados[0].Key} ";
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
    }


}
