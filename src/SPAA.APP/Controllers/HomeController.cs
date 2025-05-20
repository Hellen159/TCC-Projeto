using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;
using SPAA.APP.Models;
using SPAA.APP.ViewModels;
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
        private readonly IPreRequisitoRepository _preRequisitoRepository;


        public HomeController(ILogger<HomeController> logger,
                               IAlunoRepository alunoRepository,
                               IAlunoDisciplinaRepository alunoDisciplinaRepository,
                               IDisciplinaRepository disciplinaRepository,
                               IMapper mapper,
                               ICurriculoRepository curriculoRepository,
                               IPreRequisitoRepository preRequisitoRepository)
        {
            _logger = logger;
            _alunoRepository = alunoRepository;
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
            _disciplinaRepository = disciplinaRepository;
            _mapper = mapper;
            _curriculoRepository = curriculoRepository;
            _preRequisitoRepository = preRequisitoRepository;   
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

            var disciplinasViewModel = await ObterDisciplinasAluno(User.Identity.Name);

            ViewData["Aprovadas"] = disciplinasViewModel.Aprovadas;
            ViewData["Pendentes"] = disciplinasViewModel.Pendentes;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
