using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface ITurmaRepository : IRepository<Turma, int>
    {
        Task<List<Turma>> TurmasDisponiveisPorDisciplina(string nomeMateria);
    }
}
