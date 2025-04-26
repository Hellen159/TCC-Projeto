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
    public class TurmaRepository : Repository<Turma, string>, ITurmaRepository
    {
        public TurmaRepository(MeuDbContext context) : base(context)
        {
        }

        public async Task<List<Turma>> ObterTurmasPorCodigoDisciplina(string codigoDisciplina)
        {
            //teste de navegação entre as classes
            //var turma = await _context.Turmas
            //    .Include(t => t.Disciplina)  // Inclui a disciplina associada
            //    .FirstOrDefaultAsync(t => t.Id == 01);

            return await DbSet
                    .Where(t => t.CodigoDisciplina == codigoDisciplina)  // Filtra apenas pelo CodigoDisciplina
                    .ToListAsync();
        }
    }
}
