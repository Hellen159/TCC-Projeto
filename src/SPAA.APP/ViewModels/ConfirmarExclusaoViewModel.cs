using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class ConfirmarExclusaoViewModel
    {
        // Matrícula
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(9, MinimumLength = 1, ErrorMessage = "O campo {0} deve ter {1} caracteres.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O campo {0} deve conter exatamente 9 dígitos.")]
        [Display(Name = "Matrícula")]
        public string Matricula { get; set; }
    }
}
