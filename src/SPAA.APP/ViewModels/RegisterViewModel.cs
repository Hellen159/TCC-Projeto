using System.ComponentModel.DataAnnotations;

namespace Projeto.App.ViewModels
{
    public class RegisterViewModel
    {
        // Email
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo {0} deve conter um endereço de email válido.")]
        [StringLength(150, MinimumLength = 6, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.")]
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

        // Nome Completo
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(150, MinimumLength = 10, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        // Matrícula
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(9, MinimumLength = 1, ErrorMessage = "O campo {0} deve ter {1} caracteres.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O campo {0} deve conter exatamente 9 dígitos.")]
        [Display(Name = "Matrícula")]
        public string Matricula { get; set; }

        // Semestre de Entrada
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, 2, ErrorMessage = "O campo {0} deve ser 1 (primeiro semestre) ou 2 (segundo semestre).")]
        [Display(Name = "Semestre de Entrada")]
        public int SemestreEntrada { get; set; }

        // Ano de Entrada
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(2015, 2025, ErrorMessage = "O campo {0} deve estar entre {1} e {2}.")]
        [Display(Name = "Ano de Entrada")]
        public int AnoEntrada { get; set; }
    }
}
