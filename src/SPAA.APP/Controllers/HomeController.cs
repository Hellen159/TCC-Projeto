using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;
using SPAA.APP.Models;
using SPAA.Business.Interfaces;
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


        public HomeController(ILogger<HomeController> logger,
                               IAlunoRepository alunoRepository,
                               IAlunoDisciplinaRepository alunoDisciplinaRepository,
                               IDisciplinaRepository disciplinaRepository,
                               IMapper mapper,
                               ICurriculoRepository curriculoRepository)
        {
            _logger = logger;
            _alunoRepository = alunoRepository;
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
            _disciplinaRepository = disciplinaRepository;
            _mapper = mapper;
            _curriculoRepository = curriculoRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var alunoJaAnexouHistorico = await _alunoRepository.AlunoJaAnexouHistorico(User.Identity.Name);
            if (!alunoJaAnexouHistorico)
            {
                return RedirectToAction("UploadHistorico", "Upload");
            }

            var disciplinasViewModel = await ObterDisciplinasAlunoAsync(User.Identity.Name);

            ViewData["Aprovadas"] = disciplinasViewModel.Aprovadas;
            ViewData["Pendentes"] = disciplinasViewModel.Pendentes;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // metodos privados
        private async Task<(DisciplinaListaViewModel Aprovadas, DisciplinaListaViewModel Pendentes)> ObterDisciplinasAlunoAsync(string matricula)
        {
            var dadosAluno = await _alunoRepository.ObterPorId(matricula);

            var disciplinasCurriculoObrigatorias = await _curriculoRepository.ObterDisciplinasObrigatoriasPorCurrciulo(dadosAluno.CurriculoAluno, 1);

            var codigos = await _alunoDisciplinaRepository.ObterCodigosDisciplinasPorSituacao(matricula, "APR");

            var disciplinasAprovadas = await _disciplinaRepository
                .ObterDisciplinasPorCodigosOuEquivalentes(codigos);

            var nomeDisciplinasAprovadas = disciplinasAprovadas
                .Select(d => d.NomeDisciplina)
                .Distinct()
                .ToList();

            var obrigatoriasPendentes = disciplinasCurriculoObrigatorias
                .Where(d => !nomeDisciplinasAprovadas.Contains(d.NomeDisciplina))
                .ToList();

            var pendentesViewModel = obrigatoriasPendentes
                .Select(c => new DisciplinaViewModel
                {
                    NomeDisciplina = c.NomeDisciplina
                })
                .ToList();

            var disciplinaViewModel = _mapper.Map<List<DisciplinaViewModel>>(disciplinasAprovadas);

            var aprovadasViewModel = new DisciplinaListaViewModel
            {
                Titulo = "Disciplinas concluídas",
                Disciplinas = disciplinaViewModel
            };

            var pendentesViewModelResult = new DisciplinaListaViewModel
            {
                Titulo = "Disciplinas pendentes",
                Disciplinas = pendentesViewModel
            };

            return (aprovadasViewModel, pendentesViewModelResult);
        }

    }
}
