using Microsoft.AspNetCore.Http;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces
{
    public interface IDisciplinaRepository : IRepository<Disciplina, string>
    {
        Task<Disciplina> ObterDisciplinaPorCodigo(string codigoDisciplina);
        Task<Disciplina> ObterDisciplinaPorCodigoEquivalente(string codigoDisciplina);
        Task<List<Disciplina>> ObterDisciplinasPorCodigosOuEquivalentes(List<string> codigo);
    }
}
