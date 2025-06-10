using Microsoft.AspNetCore.Mvc;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;

namespace SPAA.App.Components

{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IAlunoRepository _alunoRepository;
        private readonly IAlunoService _alunoService;
        private readonly INotificacaoRepository _notificacaoRepository;

        public SidebarViewComponent(IAlunoRepository alunoRepository, 
                                    IAlunoService alunoService,
                                    INotificacaoRepository notificacaoRepository) 
        {
            _alunoRepository = alunoRepository;
            _alunoService = alunoService; 
            _notificacaoRepository = notificacaoRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var matricula = User.Identity.Name;

            var aluno = await _alunoRepository.ObterPorId(matricula);

            IEnumerable<Notificacao> notificacoesAtivas = await _notificacaoRepository.ObterNotificacaoPorStatus(1);
            // Se as notificações forem por aluno, você pode passar a matrícula ou o objeto aluno:
            // IEnumerable<Notificacao> notificacoesAtivas = await _notificacaoService.ObterNotificacoesAtivasParaAluno(matricula);

            ViewBag.Notificacoes = notificacoesAtivas; // Passa a lista de notificações para a View


            double porcentagemCurso = 0.0;
            if (aluno != null)
            {
                porcentagemCurso = await _alunoService.PorcentagemCurso(matricula);
            }

            ViewData["PorcentagemCurso"] = porcentagemCurso;

            return View("Default", aluno);
        }
    }
}
