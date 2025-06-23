// Crie esta classe em SPAA.Business.Models ou em uma pasta de Teste separada se preferir
// Ex: SPAA.Data.Tests.Models/TesteEntity.cs
using System.ComponentModel.DataAnnotations; // Para o [Key]

namespace SPAA.Business.Models // Ou SPAA.Data.Tests.Models, se criar a pasta
{
    public class TesteEntity
    {
        [Key] // Marca CodigoTesteEntity como a chave primária
        public int CodigoTesteEntity { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
    }
}