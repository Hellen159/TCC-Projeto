using System.ComponentModel.DataAnnotations;
namespace Projeto.App.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "A matrícula é obrigatória.")]
        public string Matricula { get; set; }


        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }
    }
}