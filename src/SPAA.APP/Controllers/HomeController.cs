using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;
using SPAA.APP.Models;
using SPAA.APP.ViewModels;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System.Diagnostics;

namespace SPAA.APP.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAlunoRepository _alunoRepository;
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        private readonly IDisciplinaRepository _disciplinaRepository;
        private readonly IMapper _mapper;
        private readonly ICurriculoRepository _curriculoRepository;
        private readonly IPreRequisitoRepository _preRequisitoRepository;
        private readonly IAreaInteresseAlunoRepository _areaInteresseAlunoRepository;
        private readonly ITurmaRepository _turmaRepository;
        private readonly IAlunoService _alunoService;
        private readonly ITurmaSalvaRepository _turmaSalvaRepository;
        private readonly ITarefaAlunoRepository _tarefaAlunoRepository;


        public HomeController(ILogger<HomeController> logger,
                               IAlunoRepository alunoRepository,
                               IAlunoDisciplinaRepository alunoDisciplinaRepository,
                               IDisciplinaRepository disciplinaRepository,
                               IMapper mapper,
                               ICurriculoRepository curriculoRepository,
                               IPreRequisitoRepository preRequisitoRepository,
                               IAreaInteresseAlunoRepository areaInteresseAlunoRepository,
                               ITurmaRepository turmaRepository,
                               IAlunoService alunoService,
                               ITurmaSalvaRepository turmaSalvaRepository,
                               ITarefaAlunoRepository tarefaAlunoRepository)
        {
            _logger = logger;
            _alunoRepository = alunoRepository;
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
            _disciplinaRepository = disciplinaRepository;
            _mapper = mapper;
            _curriculoRepository = curriculoRepository;
            _preRequisitoRepository = preRequisitoRepository;
            _areaInteresseAlunoRepository = areaInteresseAlunoRepository;
            _turmaRepository = turmaRepository;
            _alunoService = alunoService;
            _turmaSalvaRepository = turmaSalvaRepository;
            _tarefaAlunoRepository = tarefaAlunoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string success)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var alunoJaAnexouHistorico = await _alunoService.AlunoJaAnexouHistorico(User.Identity.Name);
            if (!alunoJaAnexouHistorico)
            {
                return RedirectToAction("UploadHistorico", "Upload");
            }

            var testeTodasTurmas = await _turmaRepository.TurmasDisponiveisPorSemestre("2025.1");

            var disciplinasViewModel = await ObterDisciplinasAluno(User.Identity.Name);

            var turmasSalvas = await _turmaSalvaRepository.TodasTurmasSalvasAluno(User.Identity.Name);
            var turmasSalvasViewModel = _mapper.Map<List<TurmaViewModel>>(turmasSalvas);
            var teste = await _turmaSalvaRepository.HorariosComAulas(User.Identity.Name);
            var todasAsTarefasAluno = await _tarefaAlunoRepository.TodasTarefasDoAluno(User.Identity.Name);


            ViewData["Aprovadas"] = disciplinasViewModel.Aprovadas;
            ViewData["Pendentes"] = disciplinasViewModel.Pendentes;
            ViewData["TurmasSalvas"] = turmasSalvasViewModel;
            ViewData["TarefasAluno"] = todasAsTarefasAluno;

            // Se veio da modal com sucesso
            if (!string.IsNullOrEmpty(success))
            {
                TempData["MensagemSucesso"] = "Tarefas salvas com sucesso!";
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> SalvarTarefas([FromBody] List<TarefaViewModel> tarefasRecebidas)
        {
            string matriculaAluno = User.Identity.Name;
            if (string.IsNullOrEmpty(matriculaAluno))
                return Unauthorized(new { success = false, message = "Usuário não autenticado." });

            try
            {
                // 1. Pegar todas as tarefas atuais do aluno
                var tarefasAntigas = await _tarefaAlunoRepository.TodasTarefasDoAluno(matriculaAluno);

                // 2. Apagar todas as tarefas antigas
                foreach (var tarefa in tarefasAntigas)
                {
                    await _tarefaAlunoRepository.Remover(tarefa.CodigoTarefaAluno);
                }

                // 3. Adicionar as novas tarefas
                foreach (var tarefa in tarefasRecebidas)
                {
                    await _tarefaAlunoRepository.Adicionar(new TarefaAluno
                    {
                        NomeTarefa = tarefa.Descricao,
                        Horario = tarefa.Horario,
                        Matricula = matriculaAluno
                    });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao salvar tarefas.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirTarefa([FromBody] string horarioTarefa) 
        {
            string matriculaAluno = User.Identity.Name;

            if (string.IsNullOrEmpty(matriculaAluno))
            {
                return Unauthorized(new { success = false, message = "Matrícula do aluno não pode ser determinada." });
            }

            try
            {
                int? idTarefa = await _tarefaAlunoRepository.IdTarefa(horarioTarefa, matriculaAluno);

                if (idTarefa.HasValue) 
                {

                    await _tarefaAlunoRepository.Remover(idTarefa.Value);

                    return Json(new { success = true, message = "Tarefa removida com sucesso!" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Tarefa não encontrada para os critérios fornecidos." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao tentar excluir tarefa: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Ocorreu um erro ao excluir a tarefa.", error = ex.Message });
            }
        }

        //metodos privados
        private async Task<(DisciplinaListaViewModel Aprovadas, DisciplinaListaViewModel Pendentes)> ObterDisciplinasAluno(string matricula)
        {
            var dadosAluno = await _alunoRepository.ObterPorId(matricula);

            var nomesAprovadas = await _alunoDisciplinaRepository.ObterNomeDisciplinasPorSituacao(matricula, "APR");
            var nomesPendentes = await _alunoDisciplinaRepository.ObterNomeDisciplinasPorSituacao(matricula, "PEND");


            var disciplinasAprovadasViewModel = nomesAprovadas
                .Select(nome => new DisciplinaViewModel
                {
                    NomeDisciplina = nome
                })
                .ToList();

            var disciplinasPendentesViewModel = nomesPendentes
                .Select(nome => new DisciplinaViewModel
                {
                    NomeDisciplina = nome
                })
                .ToList();

            var aprovadasViewModel = new DisciplinaListaViewModel
            {
                Titulo = "Disciplinas Aprovadas",
                Disciplinas = disciplinasAprovadasViewModel
            };

            var pendentesViewModel = new DisciplinaListaViewModel
            {
                Titulo = "Disciplinas Pendentes",
                Disciplinas = disciplinasPendentesViewModel
            };

            return (aprovadasViewModel, pendentesViewModel);
        }
    }
}
