using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface ITurmaSalvaRepository : IRepository<TurmaSalva, int>
    {
        Task<bool> ExcluirTurmasSalvasDoAluno(string matricula);
        Task<List<TurmaSalva>> TodasTurmasSalvasAluno(string matricula);
        Task<List<string>> HorariosComAulas(string matricula);

    }
}
