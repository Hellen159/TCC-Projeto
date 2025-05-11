using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class DisciplinaViewModel
    {
        public string NomeDisciplina { get; set; }
        public string CodigoDisciplina { get; set; }

        [Display(Name = "Carga Horaria")]
        public int CargaHoraria { get; set; }
    }
}
