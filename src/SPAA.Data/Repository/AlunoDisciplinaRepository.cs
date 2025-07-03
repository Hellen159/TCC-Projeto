using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SPAA.Data.Repository
{
    public class AlunoDisciplinaRepository : Repository<AlunoDisciplina, string>, IAlunoDisciplinaRepository
    {
        private readonly ILogger<AlunoDisciplinaRepository> _logger;
        private readonly IAlunoRepository _alunoRepository;
        private readonly IAlunoService _alunoService;



        public AlunoDisciplinaRepository(MeuDbContext context,
                                            ILogger<AlunoDisciplinaRepository> logger,
                                            IAlunoRepository alunoRepository,
                                            IAlunoService alunoService) : base(context)
        {
            _logger = logger;
            _alunoRepository = alunoRepository;
            _alunoService = alunoService;
        }

        public async Task<bool> ExcluirDisciplinasDoAluno(string matricula)
        {
            var disciplinasDoAluno = await DbSet
                .Where(ad => ad.Matricula == matricula)
                .ToListAsync();

            if (!disciplinasDoAluno.Any())
                return false;

            DbSet.RemoveRange(disciplinasDoAluno);
            await SaveChanges();

            return true;
        }

        public Task<List<AlunoDisciplina>> ObterAlunoDisciplinaPorSituacao(string matricula, string situacao)
        {
            return DbSet
                .Where(ad => ad.Matricula == matricula && ad.Situacao == situacao)
                .ToListAsync();
        }

        public Task<List<string>> ObterNomeDisciplinasPorSituacao(string matricula, string situacao)
        {
            return DbSet
                .Where(ad => ad.Matricula == matricula && ad.Situacao == situacao)
                .Select(ad => ad.NomeDisicplina)
                .Distinct()
                .ToListAsync();
        }

        public async Task InserirDisciplinas(List<AlunoDisciplina> disciplinas)
        {
            if (disciplinas.Any())
            {
                DbSet.AddRange(disciplinas);
                await SaveChanges();
            }
        }

        public async Task InserirEquivalencias(List<AlunoDisciplina> equivalencias, string matricula)
        {
            if (equivalencias == null || !equivalencias.Any())
            {
                return; 
            }

            foreach (var disciplina in equivalencias)
            {
                bool jaExiste = await DbSet.AnyAsync(d =>
                    d.Matricula == disciplina.Matricula &&
                    d.NomeDisicplina == disciplina.NomeDisicplina &&
                    d.Situacao == "APR");

                if (!jaExiste)
                {
                    DbSet.Add(disciplina);
                }
            }
            await SaveChanges();
        }

    }
}
