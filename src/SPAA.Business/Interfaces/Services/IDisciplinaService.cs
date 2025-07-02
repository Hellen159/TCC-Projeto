using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Services
{
    public interface IDisciplinaService
    {
        Task<List<string>> ObterDisciplinasLiberadas(string matricula);
        Task<List<string>> VerificaSeCumprePreRequisitos(List<Turma> turmas, string matricula);

    }
}
