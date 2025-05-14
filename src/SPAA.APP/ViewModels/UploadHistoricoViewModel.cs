using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class UploadFormViewModel
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        public IFormFile Historico { get; set; }
    }
}
