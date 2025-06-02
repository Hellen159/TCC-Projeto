using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class MontarGradeViewModel
    {
        [Required]
        public List<string> HorariosMarcados { get; set; } // Ex: ["6M2", "2T5"]
    }
}