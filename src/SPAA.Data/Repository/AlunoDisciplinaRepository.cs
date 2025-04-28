using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System.Text;
using System.Text.RegularExpressions;

namespace SPAA.Data.Repository
{
    public class AlunoDisciplinaRepository : Repository<AlunoDisciplina, string>, IAlunoDisciplinaRepository
    {
        private readonly ILogger<AlunoDisciplinaRepository> _logger;
        private readonly IDisciplinaRepository _disciplinaRepository;
        private readonly IAlunoRepository _alunoRepository;

        public AlunoDisciplinaRepository(MeuDbContext context,
                                            ILogger<AlunoDisciplinaRepository> logger,
                                            IAlunoRepository alunoRepository,
                                            IDisciplinaRepository disciplinaRepository) : base(context)
        {
            _logger = logger;
            _disciplinaRepository = disciplinaRepository;
            _alunoRepository = alunoRepository;
        }

        public async Task<(bool isValid, string mensagem)> ConsumirHistoricoPdf(IFormFile arquivoPdf, string matricula)
        {
            if (arquivoPdf == null || arquivoPdf.Length == 0)
                return (false, "Nenhum arquivo enviado.");

            try
            {
                List<string> codigosNaoEncontrados = new();
                string textoFormatado = await ExtrairTextoDoPdf(arquivoPdf);

                // Processamento das informações do histórico
                var curriculoAno = ObterInformacoesCurriculo(textoFormatado);
                if (curriculoAno != null)
                {
                    _logger.LogInformation($"Currículo: {curriculoAno}");
                }

                var disciplinas = ExtrairDisciplinasDoTexto(textoFormatado);

                if (!disciplinas.Any())
                    return (false, "Formato de histórico inválido ou nenhum dado encontrado.");

                List<AlunoDisciplina> entradas = await ProcessarDisciplinas(matricula, disciplinas, codigosNaoEncontrados);

                if (entradas.Any())
                {
                    DbSet.AddRange(entradas);
                    await SaveChanges();
                }
                
                var (anexadoComSucesso, mensagemAnexar) = await _alunoRepository.MarcarHistoricoComoAnexado(matricula);
                if (!anexadoComSucesso)
                {
                    return (false, mensagemAnexar);
                }

                if (codigosNaoEncontrados.Any() && anexadoComSucesso)
                {
                    var codigos = string.Join(", ", codigosNaoEncontrados.Distinct());
                    var mensagem = $"{mensagemAnexar} No entanto, as seguintes disciplinas não foram encontradas no sistema: {codigos}";
                    return (true, mensagem);
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

        private async Task<string> ExtrairTextoDoPdf(IFormFile arquivoPdf)
        {
            using var stream = arquivoPdf.OpenReadStream();
            using var reader = new PdfReader(stream);
            using var document = new PdfDocument(reader);

            StringBuilder fullText = new StringBuilder();
            for (int i = 1; i <= document.GetNumberOfPages(); i++)
            {
                var page = document.GetPage(i);
                fullText.Append(PdfTextExtractor.GetTextFromPage(page));
            }

            return PadronizarTextoHistorico(fullText.ToString());
        }

        private string ObterInformacoesCurriculo(string texto)
        {
            var regexCurriculo = new Regex(@"^Currículo:\s*\d+\/\d+\s*-\s*\|\s*(?<ano>\d{4})\.(?<semestre>\d)", RegexOptions.Multiline);
            var match = regexCurriculo.Match(texto);

            if (match.Success)
            {
                return $"{match.Groups["ano"].Value}.{match.Groups["semestre"].Value}";
            }

            return null;
        }

        private List<(string semestre, string codigo, string situacao)> ExtrairDisciplinasDoTexto(string texto)
        {
            var regex = new Regex(
                @"\|\s(?<semestre>\d{4}\.\d).*?(?<codigo>[A-Z]{3,}\d{4}).*?\b(?<situacao>APR|CANC|DISP|MATR|REP|REPF|REPMF|TRANC|CUMP)\b\s?\|",
                RegexOptions.Compiled
            );

            var matches = regex.Matches(texto);
            var disciplinas = new List<(string semestre, string codigo, string situacao)>();

            foreach (Match match in matches)
            {
                disciplinas.Add((
                    match.Groups["semestre"].Value,
                    match.Groups["codigo"].Value,
                    match.Groups["situacao"].Value
                ));
            }

            return disciplinas;
        }

        private async Task<List<AlunoDisciplina>> ProcessarDisciplinas(string matricula, List<(string semestre, string codigo, string situacao)> disciplinas, List<string> codigosNaoEncontrados)
        {
            var entradas = new List<AlunoDisciplina>();

            foreach (var (semestre, codigo, situacao) in disciplinas)
            {
                var disciplina = await _disciplinaRepository.ObterDisciplinaPorCodigo(codigo);
                if (disciplina != null)
                {
                    var entradaExistente = await ObterPorChaveComposta(matricula, codigo, semestre);
                    if (entradaExistente == null)
                    {
                        var entrada = new AlunoDisciplina
                        {
                            Matricula = matricula,
                            CodigoDisciplina = codigo,
                            Semestre = semestre,
                            Situacao = situacao
                        };
                        entradas.Add(entrada);
                    }
                }
                else
                {
                    _logger.LogWarning("Disciplina com código '{Codigo}' não encontrada ", codigo);
                    codigosNaoEncontrados.Add(codigo);
                }
            }

            return entradas;
        }

        private string PadronizarTextoHistorico(string textoOriginal)
        {
            if (string.IsNullOrWhiteSpace(textoOriginal)) return textoOriginal;

            var textoPadronizado = Regex.Replace(textoOriginal, @"(^|\s)(\d{4}\.\d)", "$1| $2");
            string[] siglasSituacao = { "APR", "CANC", "DISP", "MATR", "REP", "REPF", "REPMF", "TRANC", "CUMP" };
            string padraoSituacoes = @"\b(" + string.Join("|", siglasSituacao) + @")\b(?!\s*\|)";
            textoPadronizado = Regex.Replace(textoPadronizado, padraoSituacoes, "$1 |");

            return textoPadronizado;
        }

        public async Task<AlunoDisciplina> ObterPorChaveComposta(string matricula, string codigoDisciplina, string semestre)
        {
            return await DbSet.FindAsync(matricula, codigoDisciplina, semestre);
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
    }
}
