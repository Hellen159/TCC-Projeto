using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System.Text.RegularExpressions;

namespace SPAA.Data.Repository
{
    public class DisciplinaRepository : Repository<Disciplina, string>, IDisciplinaRepository
    {
        public DisciplinaRepository(MeuDbContext context) : base(context)
        {
        }

        public async Task<Disciplina> ObterPorCodigo(string codigo)
        {
            return await DbSet.FirstOrDefaultAsync(d => d.Codigo == codigo);
        }
    }
}
