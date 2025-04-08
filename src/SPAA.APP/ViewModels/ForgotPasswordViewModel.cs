using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }
    }
}
