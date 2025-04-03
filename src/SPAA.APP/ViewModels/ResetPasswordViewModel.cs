using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }
        // Senha
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        // Confirmação da Senha
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        [Display(Name = "Confirme a Senha")]
        public string ConfirmacaoSenha { get; set; }

        public string Token { get; set; }
    }
}
