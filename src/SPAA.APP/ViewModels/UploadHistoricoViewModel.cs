using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class UploadHistoricoViewModel
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        public IFormFile Historico { get; set; }
    }
}
