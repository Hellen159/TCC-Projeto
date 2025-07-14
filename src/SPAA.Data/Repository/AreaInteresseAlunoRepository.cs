using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class AreaInteresseAlunoRepository : Repository<AreaInteresseAluno, int>, IAreaInteresseAlunoRepository
    {
        public AreaInteresseAlunoRepository(MeuDbContext context) : base(context)
        {
        }

        public async Task<bool> AlunoJaTemAreaInteresse(string matricula)
        {
            return await DbSet.AnyAsync(ai => ai.Matricula == matricula);
        }

        public async Task<bool> ExcluirAreaInteresseAluno (string matricula)
        {
            var areaInteresseAluno = await DbSet
                .Where(aia => aia.Matricula == matricula)
                .ToListAsync();

            if (!areaInteresseAluno.Any())
                return false;

            DbSet.RemoveRange(areaInteresseAluno);
            await SaveChanges();

            return true;
        }
    }
}
