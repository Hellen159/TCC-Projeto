using System.ComponentModel.DataAnnotations;

namespace SPAA.App.ViewModels
{
    public class ConfigurationViewModel
    {

        public IFormFile Historico { get; set; }

        [DataType(DataType.Password)]
        public string SenhaAtual { get; set; }


        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmacaoSenha { get; set; }

        // Novo Nome 
        public string NovoNome { get; set; }
    }
}
