using Microsoft.AspNetCore.Http;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces
{
    public interface IAlunoDisciplinaRepository : IRepository<AlunoDisciplina, string>
    {
        Task<(bool isValid, string mensagem)> ConsumirHistoricoPdf(IFormFile pdf, string matricula);
        Task<bool> ExcluirDisciplinasDoAluno(string matricula);

    }
}
