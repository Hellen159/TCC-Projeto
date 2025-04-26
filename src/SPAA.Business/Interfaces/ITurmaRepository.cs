using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces
{
    public interface ITurmaRepository : IRepository<Turma, string>
    {
        Task <List<Turma>> ObterTurmasPorCodigoDisciplina(string codigoDisciplina);
    }
}
