using System.ComponentModel.DataAnnotations;

namespace Projeto.App.ViewModels
{
    public class RegisterViewModel
    {
        // ApplicationUser

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Senha { get; set; }



        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        [Display(Name = "Confirme a senha")]
        public string ConfirmacaoSenha { get; set; }

        //Aluno
        [Required]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        [Required]
        [Display(Name = "Matricula")]
        public string Matricula { get; set; }

        [Required]
        [Display(Name = "Semestre de entrada")]
        public string SemestreEntrada { get; set; }
    }
}
