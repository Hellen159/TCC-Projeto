using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPAA.Business.Services
{
    public class AlunoDisciplinaService : IAlunoDisciplinaService
    {

        private static readonly string[] SiglasSituacoes = new[] { "APR", "CANC", "DISP", "MATR", "REP", "REPF", "REPMF", "TRANC", "CUMP" };
        private static readonly Regex semestreAnoRegex = new Regex(@"\b(\d{4})\.(\d{0,2})\b");
        private readonly ILogger<AlunoDisciplinaService> _logger;
        private readonly IAlunoService _alunoService;
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        public AlunoDisciplinaService(ILogger<AlunoDisciplinaService> logger,
                                      IAlunoService alunoService,
                                      IAlunoDisciplinaRepository alunoDisciplinaRepository)
        {
            _logger = logger;
            _alunoService = alunoService;
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
        }

        public async Task<List<AlunoDisciplina>> ConverterBlocos(List<string> blocos, string matricula)
        {
            var disciplinas = new List<AlunoDisciplina>();

            foreach (var bloco in blocos)
            {
                var disciplina = ProcessarBloco(bloco, matricula);
                if (disciplina != null)
                {
                    disciplinas.Add(disciplina);
                }
            }
            return disciplinas;
        }

        public async Task<List<string>> ExtrairBlocos(string texto)
        {
            var blocos = new List<string>();
            var matchesSemestres = semestreAnoRegex.Matches(texto);
            string padraoSituacoes = string.Join("|", SiglasSituacoes);
            var regexSituacaoFinal = new Regex(@"\b(" + padraoSituacoes + @")\b");

            for (int i = 0; i < matchesSemestres.Count; i++)
            {
                int inicio = matchesSemestres[i].Index;
                int limiteBusca = (i + 1 < matchesSemestres.Count) ? matchesSemestres[i + 1].Index : texto.Length;
                string trecho = texto.Substring(inicio, limiteBusca - inicio);

                MatchCollection situacoes = regexSituacaoFinal.Matches(trecho);
                if (situacoes.Count > 0)
                {
                    Match ultimaSituacao = situacoes[^1];
                    int fim = ultimaSituacao.Index + ultimaSituacao.Length;
                    string bloco = trecho.Substring(0, fim).Trim();
                    blocos.Add(bloco);
                }
            }

            return blocos;
        }

        public virtual async Task<string> ExtrairTextoDePdf(IFormFile arquivoPdf)
        {
            using var memoryStream = new MemoryStream();
            await arquivoPdf.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var stringBuilder = new StringBuilder();

            using var pdfReader = new PdfReader(memoryStream);
            using var pdfDoc = new PdfDocument(pdfReader);

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var pagina = pdfDoc.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var texto = PdfTextExtractor.GetTextFromPage(pagina, strategy);
                stringBuilder.AppendLine(texto);
            }

            return stringBuilder.ToString();
        }

        public async Task<List<AlunoDisciplina>> ObterEquivalenciasCurriculo(string texto, string matricula)
        {
            var idxEquivalencias = texto.IndexOf("Equivalências:", StringComparison.OrdinalIgnoreCase);
            if (idxEquivalencias < 0)
                return null;

            var trecho = texto.Substring(idxEquivalencias);
            var regexCumpriu = new Regex(@"Cumpriu.*\)\s*");
            var matches = regexCumpriu.Matches(trecho);

            if (matches.Count == 0)
                return null;

            var equivalencias = new List<AlunoDisciplina>();

            foreach (Match match in matches)
            {
                string matchTexto = match.Value;
                int posicaoPrimeiroHifen = matchTexto.IndexOf(" -");
                int posicaoPrimeiroParentese = matchTexto.IndexOf('(');

                if (posicaoPrimeiroParentese > posicaoPrimeiroHifen)
                {
                    string nomeDisciplina = matchTexto.Substring(posicaoPrimeiroHifen + 2, posicaoPrimeiroParentese - posicaoPrimeiroHifen - 2).Trim();

                    equivalencias.Add(new AlunoDisciplina
                    {
                        Semestre = "0000.0",
                        Situacao = "APR",
                        NomeDisicplina = NormalizarTexto(nomeDisciplina),
                        Matricula = matricula
                    });
                }
            }

            return equivalencias;
        }

        public async Task<string> ObterInformacoesCurriculo(string texto)
        {
            var idxCurriculo = texto.IndexOf("Currículo:", StringComparison.OrdinalIgnoreCase);
            if (idxCurriculo < 0)
                return null;

            var trecho = texto.Substring(idxCurriculo);

            var match = semestreAnoRegex.Match(trecho);
            return match.Success ? match.Value : null;
        }

        public async Task<List<AlunoDisciplina>> ObterObrigatoriasPendentes(string texto, string matricula)
        {
            var resultado = new List<AlunoDisciplina>();

            var titulo = "Componentes Curriculares Obrigatórios Pendentes:";
            var idxInicio = texto.IndexOf(titulo, StringComparison.OrdinalIgnoreCase);

            if (idxInicio == -1)
                return resultado;

            var trecho = texto.Substring(idxInicio + titulo.Length);

            var idxFim = trecho.IndexOf("Componentes Curriculares", 10, StringComparison.OrdinalIgnoreCase);
            if (idxFim > 0)
                trecho = trecho.Substring(0, idxFim);

            var linhas = trecho.Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) && Regex.IsMatch(l, @"\d{2,3} h\s+[A-Z]{3}\d{4}"))
                .ToList();

            foreach (var linha in linhas)
            {
                var linhaLimpa = Regex.Replace(linha, @"\s+Matriculado(\s+em\s+Equivalente)?", "", RegexOptions.IgnoreCase);

                var match = Regex.Match(linhaLimpa, @"^(.*?)\s+\d{1,3} h\s+[A-Z]{3}\d{4}$");
                if (match.Success)
                {
                    var nome = match.Groups[1].Value.Trim();

                    resultado.Add(new AlunoDisciplina
                    {
                        NomeDisicplina = NormalizarTexto(nome),
                        Situacao = "PEND",
                        Matricula = matricula,
                        Semestre = "0000.0"
                    });
                }
            }

            return resultado;
        }

        private AlunoDisciplina ProcessarBloco(string bloco, string matricula)
        {
            var matchSemestre = semestreAnoRegex.Match(bloco);

            if (!matchSemestre.Success)
                return null;

            var ano = matchSemestre.Groups[1].Value;
            var periodo = matchSemestre.Groups[2].Success ? matchSemestre.Groups[2].Value : "1";
            var semestre = $"{ano}.{periodo}";
            string padraoSituacoes = string.Join("|", SiglasSituacoes);
            var regexSituacao = new Regex(@"\b(" + padraoSituacoes + @")\b");
            var situacaoMatch = regexSituacao.Match(bloco);

            if (!situacaoMatch.Success)
                return null;

            string situacao = situacaoMatch.Value;
            var linhas = bloco.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string nome;

            if (linhas.Length == 1)
            {
                var palavras = linhas[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                int idxSemestre = Array.FindIndex(palavras, p => p.Contains(ano));
                int idxPenultima = palavras.Length - 2;

                if (idxSemestre >= 0 && idxPenultima > idxSemestre)
                {
                    nome = string.Join(" ", palavras[(idxSemestre + 1)..idxPenultima]);
                }
                else
                {
                    nome = string.Empty;
                }
            }
            else
            {
                nome = linhas[1].Trim();
            }
            return new AlunoDisciplina
            {
                Semestre = semestre,
                Situacao = situacao,
                NomeDisicplina = NormalizarTexto(nome),
                Matricula = matricula
            };
        }

        public static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            texto = texto.ToUpperInvariant();

            var textoNormalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in textoNormalizado)
            {
                var unicodeCategoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategoria != UnicodeCategory.NonSpacingMark && c != 'Ç')
                {
                    sb.Append(c);
                }
                else if (c == 'Ç')
                {
                    sb.Append('C');
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public async Task<(bool isValid, string mensagem)> ConsumirHistoricoPdf(IFormFile arquivoPdf, string matricula)
        {
            if (arquivoPdf == null || arquivoPdf.Length == 0)
                return (false, "Nenhum arquivo enviado.");

            try
            {
                List<string> codigosNaoEncontrados = new();
                string textoExtraido = await ExtrairTextoDePdf(arquivoPdf);
                var horasPendentes = await ObterHorasPendentes(textoExtraido, matricula);
                var blocosDisciplinas = await ExtrairBlocos(textoExtraido);
                var listaEquivalencia = await ObterEquivalenciasCurriculo(textoExtraido, matricula);
                var listaAlunoDisciplina = await ConverterBlocos(blocosDisciplinas, matricula);
                var listaPendentes = await ObterObrigatoriasPendentes(textoExtraido, matricula);

                var curriculoAno = await ObterInformacoesCurriculo(textoExtraido);

                if (curriculoAno != null)
                {
                    _logger.LogInformation($"Currículo: {curriculoAno}");
                    await _alunoService.AdicionarCurriculoAluno(matricula, curriculoAno);
                }


                if (!listaAlunoDisciplina.Any())
                    return (false, "Formato de histórico inválido ou nenhum dado encontrado.");


                await _alunoDisciplinaRepository.InserirDisciplinas(listaAlunoDisciplina);
                await _alunoDisciplinaRepository.InserirDisciplinas(listaPendentes);
                await _alunoDisciplinaRepository.InserirEquivalencias(listaEquivalencia, matricula);



                var (anexadoComSucesso, mensagemAnexar) = await _alunoService.MarcarHistoricoComoAnexado(matricula);
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

        public async Task<List<int>> ObterHorasPendentes(string texto, string matricula)
        {
            var marcadorInicio = "Exigido\nIntegralizado\nPendente";
            var idxInicio = texto.IndexOf(marcadorInicio, StringComparison.OrdinalIgnoreCase);
            if (idxInicio < 0)
                return new List<int>(); 

            var trecho = texto.Substring(idxInicio);

            var regexHoras = new Regex(@"\d+\s?h", RegexOptions.IgnoreCase);
            var matches = regexHoras.Matches(trecho);

            if (matches.Count < 6)
                return new List<int>(); 

            int horasOptativasPendentes = int.Parse(matches[2].Value.Replace("h", "").Trim());
            int horasObrigatoriasPendentes = int.Parse(matches[5].Value.Replace("h", "").Trim());

            await _alunoService.SalvarHorasAluno(matricula, horasOptativasPendentes, horasObrigatoriasPendentes);

            return new List<int>
            {
                horasOptativasPendentes,
                horasObrigatoriasPendentes
            };
        }


    }
}
