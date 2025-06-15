using SPAA.Business.Models;

namespace SPAA.App.ViewModels
{
    public class MontarGradeResultViewModel
    {
        public string Mensagem { get; set; }
        public List<TurmaViewModel> Turmas { get; set; }
        public List<TurmaViewModel> TurmasOptativas { get; set; } 
    }
}
