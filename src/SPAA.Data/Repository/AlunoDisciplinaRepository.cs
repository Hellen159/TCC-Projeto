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
        private readonly IAlunoDisciplinaService _alunoDisciplinaService;



        public AlunoDisciplinaRepository(MeuDbContext context,
                                            ILogger<AlunoDisciplinaRepository> logger,
                                            IAlunoRepository alunoRepository,
                                            IAlunoDisciplinaService alunoDisciplinaService) : base(context)
        {
            _logger = logger;
            _alunoRepository = alunoRepository;
            _alunoDisciplinaService = alunoDisciplinaService;
        }

        public async Task<(bool isValid, string mensagem)> ConsumirHistoricoPdf(IFormFile arquivoPdf, string matricula)
        {
            if (arquivoPdf == null || arquivoPdf.Length == 0)
                return (false, "Nenhum arquivo enviado.");

            try
            {
                List<string> codigosNaoEncontrados = new();
                string textoExtraido = await _alunoDisciplinaService.ExtrairTextoDePdf(arquivoPdf);
                var blocosDisciplinas = await _alunoDisciplinaService.ExtrairBlocos(textoExtraido);
                var listaEquivalencia = await _alunoDisciplinaService.ObterEquivalenciasCurriculo(textoExtraido, matricula);
                var listaAlunoDisciplina = await _alunoDisciplinaService.ConverterBlocos(blocosDisciplinas, matricula);
                var listaPendentes = await _alunoDisciplinaService.ObterObrigatoriasPendentes(textoExtraido, matricula);

                var curriculoAno = await _alunoDisciplinaService.ObterInformacoesCurriculo(textoExtraido);
                if (curriculoAno != null)
                {
                    _logger.LogInformation($"Currículo: {curriculoAno}");
                    await _alunoRepository.AdicionarCurriculoAluno(matricula, curriculoAno);
                }


                if (!listaAlunoDisciplina.Any())
                    return (false, "Formato de histórico inválido ou nenhum dado encontrado.");


                if (listaAlunoDisciplina.Any())
                {
                    DbSet.AddRange(listaAlunoDisciplina);
                    await SaveChanges();
                }

                if (listaPendentes.Any())
                {
                    DbSet.AddRange(listaPendentes);
                    await SaveChanges();
                }

                if (listaEquivalencia.Any())
                {
                    foreach (var disciplina in listaEquivalencia)
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

                var (anexadoComSucesso, mensagemAnexar) = await _alunoRepository.MarcarHistoricoComoAnexado(matricula);
                if (!anexadoComSucesso)
                {
                    return (false, mensagemAnexar);
                }

                return (true, mensagemAnexar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar o histórico do aluno.");
                var inner = ex.InnerException?.ToString() ?? "sem inner exception";
                return (false, $"Erro ao processar o arquivo: {ex.Message}. Detalhes internos: {inner}");
            }
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
    }
}
