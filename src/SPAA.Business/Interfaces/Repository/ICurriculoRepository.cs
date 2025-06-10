using SPAA.Business.Models;

namespace SPAA.Business.Interfaces.Repository
{
    public interface ICurriculoRepository : IRepository<Curriculo, int>
    {
        Task<List<Curriculo>> ObterDisciplinasPorCurrciulo(string curriculo, int tipoDisciplina);

    }
}
