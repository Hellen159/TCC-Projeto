using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SPAA.Data.Repository
{
    public class CurriculoRepository : Repository<Curriculo, int>, ICurriculoRepository
    {
        public CurriculoRepository(MeuDbContext context) : base(context)
        {
        }

        public async Task<List<Curriculo>> ObterDisciplinasObrigatoriasPorCurrciulo(string curriculo, int tipoDisciplina)
        {
            return await DbSet
                .Where(c => c.AnoCurriculo == curriculo && c.TipoDisciplina == tipoDisciplina)
                .ToListAsync();
        }
    }
}
