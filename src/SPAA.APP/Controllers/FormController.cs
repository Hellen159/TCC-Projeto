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
                return BadRequest("Nenhuma resposta recebida.");

            var perfilNotas = new Dictionary<string, List<int>>();
            var terciarios = new Dictionary<string, List<int>>();

            foreach (var resp in respostas)
            {
                if (resp.Perfil.StartsWith("Engenharia"))
                {
                    if (!terciarios.ContainsKey(resp.Perfil))
                        terciarios[resp.Perfil] = new List<int>();

                    terciarios[resp.Perfil].Add(resp.Nota);
                }
                else
                {
                    if (!perfilNotas.ContainsKey(resp.Perfil))
                        perfilNotas[resp.Perfil] = new List<int>();

                    perfilNotas[resp.Perfil].Add(resp.Nota);
                }
            }

            var principais = perfilNotas
                .Select(p => new { Perfil = p.Key, Media = p.Value.Average() })
                .OrderByDescending(p => p.Media)
                .ToList();

            var principal = principais.FirstOrDefault();
            var secundarios = principais.Skip(1).Where(p => p.Media >= 7).ToList();

            var terciariosSelecionados = terciarios
                .Select(p => new { Perfil = p.Key, Media = p.Value.Average() })
                .Where(p => p.Media >= 7)
                .ToList();

            // Salvar no banco se necessário
            // _repositorio.SalvarPerfil(usuarioId, principal.Perfil, secundarios, terciariosSelecionados);

            return Json(new
            {
                principal = principal != null ? $"{principal.Perfil} (média {principal.Media:F2})" : "Nenhum perfil detectado.",
                secundarios = secundarios.Any()
                    ? string.Join(", ", secundarios.Select(p => $"{p.Perfil} ({p.Media:F2})"))
                    : "Nenhum perfil secundário detectado."
            });
        }
    }


}
