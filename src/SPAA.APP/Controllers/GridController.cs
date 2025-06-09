using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using System.Text.Json; // no topo do arquivo

namespace SPAA.App.Controllers
{
    public class GridController : Controller
    {
        private readonly IDisciplinaService _disciplinaService;
        private readonly ITurmaService _turmaService;

        public GridController(IDisciplinaService disciplinaService,
                               ITurmaService turmaService)
        {
            _disciplinaService = disciplinaService;
            _turmaService = turmaService;
        }

        [HttpGet]
        public IActionResult MontarGrade()
        {
            var modelVazio = new MontarGradeResultViewModel();
            return View("~/Views/DashBoard/MontarGrade.cshtml", modelVazio);
        }

        [HttpPost]
        public async Task<IActionResult> MontarGrade(MontarGradeViewModel model)
        {

            var resultado = new MontarGradeResultViewModel();

            if (ModelState.IsValid)
            {
                var preferencias = new List<AulaHorario>();

                try
                {
                    var horarios = JsonSerializer.Deserialize<List<string>>(model.HorariosMarcados);

                    foreach (var horarioString in horarios)
                    {
                        string cleaned = horarioString.Replace(" ", "").ToUpper();

                        if (cleaned.Length >= 3 &&
                            int.TryParse(cleaned[0].ToString(), out int diaSemana) &&
                            char.IsLetter(cleaned[1]) &&
                            int.TryParse(cleaned.Substring(2), out int horario))
                        {
                            preferencias.Add(new AulaHorario
                            {
                                DiaSemana = diaSemana,
                                Turno = cleaned[1],
                                Horario = horario
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Erro ao interpretar: '{horarioString}'");
                        }
                    }

                    var disciplinasPendentes = await _disciplinaService.ObterDisciplinasLiberadas(User.Identity.Name);

                    if (disciplinasPendentes == null || !disciplinasPendentes.Any())
                    {
                        resultado.Mensagem = "Parabéns! Você não possui disciplinas obrigatórias pendentes.";
                        resultado.Turmas = new List<Turma>();
                    }
                    else
                    {
                        var turmasAptas = new List<Turma>();

                        foreach (var disciplina in disciplinasPendentes)
                        {
                            var turmas = await _turmaService.BuscarTurmasCompativeis(disciplina, preferencias);
                            turmasAptas.AddRange(turmas);
                        }
                        
                        if (!turmasAptas.Any())
                        {
                            resultado.Mensagem = "Nenhuma turma compatível foi encontrada. Por isso, estamos exibindo todas as turmas para as quais você já possui os pré-requisitos. Selecione a turma desejada e clique em “Salvar turma”.";
                        }
                        else
                        {
                            resultado.Mensagem = "Turmas encontradas com sucesso. Selecione a turma desejada e clique em “Salvar turma”.";
                            resultado.Turmas = turmasAptas;
                        }


                    }
                }
                catch (JsonException ex)
                {
                    resultado.Mensagem = "Erro ao interpretar os horários enviados.";
                    Console.WriteLine($"Erro de JSON: {ex.Message}");
                }
            }
            else
            {
                resultado.Mensagem = "Erro: Dados inválidos recebidos!";
            }

            return View("~/Views/DashBoard/MontarGrade.cshtml", resultado);
        }
    }
}