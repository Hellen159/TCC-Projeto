using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface IAreaInteresseAlunoRepository : IRepository<AreaInteresseAluno, int>
    {
        Task<bool> AlunoJaTemAreaInteresse(string matricula);
        Task<bool> ExcluirAreaInteresseAluno(string matricula);
    }
}
