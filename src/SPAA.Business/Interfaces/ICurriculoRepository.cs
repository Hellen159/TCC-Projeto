using SPAA.Business.Models;

namespace SPAA.Business.Interfaces
{
    public interface ICurriculoRepository : IRepository<Curriculo, int>
    {
        Task<List<Curriculo>> ObterDisciplinasObrigatoriasPorCurrciulo(string curriculo, int tipoDisciplina);
    }
}
