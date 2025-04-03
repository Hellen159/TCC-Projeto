using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class AlunoRepository : Repository<Aluno, string>, IAlunoRepository 
    {
        public AlunoRepository(MeuDbContext context) : base(context) { }

        public async Task<string> ObterIdentityUserIdPorMatricula(string matricula)
        {
            var entidade = await ObterPorId(matricula);

            if (entidade is Aluno aluno)
            {
                return aluno.User.Id;
            }

            throw new Exception("Entidade não encontrada ou não é um Aluno.");
        }
    }
}
