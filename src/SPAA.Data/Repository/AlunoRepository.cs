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
    public class AlunoRepository : IAlunoRepository
    {
        private readonly MeuDbContext _context;

        public AlunoRepository(MeuDbContext context)
        {
            _context = context;
        }

        public async Task CriarAluno(Aluno aluno)
        {
            _context.Alunos.Add(aluno);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
