using System.ComponentModel.DataAnnotations;
namespace Projeto.App.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Matricula { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Senha { get; set; }
    }
}