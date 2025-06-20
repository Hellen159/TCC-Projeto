// Crie esta classe em SPAA.Data.Tests.Repository/TesteRepository.cs
using SPAA.Business.Models; // Ou SPAA.Data.Tests.Models
using SPAA.Data.Context;
using SPAA.Data.Repository; // Importa o namespace do seu Repository genérico

namespace SPAA.Data.Tests.Repository
{
    public class TesteRepository : Repository<TesteEntity, int>
    {
        public TesteRepository(MeuDbContext context) : base(context)
        {
        }
    }
}