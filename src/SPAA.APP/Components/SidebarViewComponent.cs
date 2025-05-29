using Microsoft.AspNetCore.Mvc;
using SPAA.Business.Interfaces.Repository;

namespace SPAA.App.Components

{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IAlunoRepository _alunoRepository;

        public SidebarViewComponent(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var matricula = User.Identity.Name;

            var aluno = await _alunoRepository.ObterPorId(matricula); 

            return View("Default", aluno);
        }
    }
}
