using Microsoft.AspNetCore.Http;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface IAlunoDisciplinaRepository : IRepository<AlunoDisciplina, string>
    {
        Task InserirEquivalencias(List<AlunoDisciplina> equivalencias, string matricula);
        Task InserirDisciplinas(List<AlunoDisciplina> disciplinas);
        Task<bool> ExcluirDisciplinasDoAluno(string matricula);
        Task<List<AlunoDisciplina>> ObterAlunoDisciplinaPorSituacao(string matricula, string situacao);
        Task<List<string>> ObterNomeDisciplinasPorSituacao(string matricula, string situacao);
    }
}
