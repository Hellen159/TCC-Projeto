﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Data.Context;

namespace SPAA.Data.Repository
{
    public class TurmaSalvaRepository : Repository<TurmaSalva, int>, ITurmaSalvaRepository
    {
        public TurmaSalvaRepository(MeuDbContext context) : base(context)
        {
        }

        public async Task<bool> ExcluirTurmasSalvasDoAluno(string matricula)
        {
            var turmasSalvas = await DbSet
                 .Where(ts=> ts.Matricula == matricula)
                 .ToListAsync();

            if (!turmasSalvas.Any())
                return false;

            DbSet.RemoveRange(turmasSalvas);
            await SaveChanges();

            return true;
        }

        public async Task<List<string>> HorariosComAulas(string matricula)
        {
            var horarios = await DbSet
                .Where(ts => ts.Matricula == matricula && ts.Horario != null) 
                .Select(ts => ts.Horario) 
                .Distinct() 
                .ToListAsync(); 

            return horarios;
        }

        public async Task<List<TurmaSalva>> TodasTurmasSalvasAluno(string matricula)
        {
            var turmasSalvas = new List<TurmaSalva>();
            turmasSalvas = await DbSet
                 .Where(ts => ts.Matricula == matricula)
                 .ToListAsync();

            if (!turmasSalvas.Any())
                return turmasSalvas;

            return turmasSalvas;
        }
    }
}
