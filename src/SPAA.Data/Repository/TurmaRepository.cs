using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Business.Services;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class TurmaRepository : Repository<Turma, int>, ITurmaRepository
    {
        public TurmaRepository(MeuDbContext context) : base(context){ }

        public async Task<List<Turma>> TurmasDisponiveisPorDisciplina(string nomeMateria)
        {
            return await DbSet.Where(t => t.NomeDisciplina == nomeMateria).ToListAsync();
        }

        public async Task<List<Turma>> TurmasDisponiveisPorSemestre(string semestre)
        {
            return await DbSet.Where(t => t.Semestre == semestre).ToListAsync();
        }
    }
}
