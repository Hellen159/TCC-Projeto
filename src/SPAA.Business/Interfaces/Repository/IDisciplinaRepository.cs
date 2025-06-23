using Microsoft.AspNetCore.Http;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface IDisciplinaRepository : IRepository<Disciplina, int>
    {
        Task<Disciplina> ObterPorCodigo(string codigo);
    }
}
