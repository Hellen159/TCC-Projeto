using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class PreRequisitoRepository : Repository<PreRequisito, int>, IPreRequisitoRepository
    {
        public PreRequisitoRepository(MeuDbContext context) : base(context)
        {
        }

        public async Task<bool> AtendeRequisitos(string expressao, List<string> disciplinasAprovadas)
        {
            if (string.IsNullOrWhiteSpace(expressao))
                return true;

            // Passo 1: Extrair os nomes das disciplinas da expressão
            var nomesDisciplinas = ExtrairDisciplinas(expressao);

            // Passo 2: Para cada nome, verificar se está na lista de aprovadas
            var mapaDisciplinas = new Dictionary<string, string>();
            foreach (var nome in nomesDisciplinas)
            {
                var aprovado = disciplinasAprovadas.Any(d => string.Equals(d.Trim(), nome.Trim(), StringComparison.OrdinalIgnoreCase));
                mapaDisciplinas[nome] = aprovado ? "true" : "false";
            }

            // Passo 3: Substituir os nomes na expressão original pelo valor true/false
            var expressaoAvaliar = expressao;
            foreach (var par in mapaDisciplinas.OrderByDescending(p => p.Key.Length)) // substituir nomes maiores primeiro para evitar substring
            {
                var nomeEscapado = Regex.Escape(par.Key);
                // Substituir só o nome completo como token, para evitar substituir dentro de outras palavras
                expressaoAvaliar = Regex.Replace(
                    expressaoAvaliar,
                    $@"\b{nomeEscapado}\b",
                    par.Value,
                    RegexOptions.IgnoreCase);
            }

            // Passo 4: Ajustar operadores lógicos para o DataTable.Compute
            expressaoAvaliar = expressaoAvaliar
                .Replace("&&", " AND ")
                .Replace("||", " OR ");

            try
            {
                var table = new DataTable();
                var resultado = (bool)table.Compute(expressaoAvaliar, "");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao avaliar expressão: " + ex.Message);
                return false;
            }
        }

        private HashSet<string> ExtrairDisciplinas(string expressao)
        {
            // Regexp para pegar sequências de caracteres que não sejam parênteses, AND, OR e operadores lógicos
            // Como os nomes tem espaços, números e caracteres, pegamos tudo entre parênteses e operadores

            var disciplinas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Limpa os parênteses, operadores lógicos, deixando só os nomes separados por || e &&
            var textoLimpo = expressao
                .Replace("(", " ")
                .Replace(")", " ")
                .Replace("&&", "||"); // normaliza para um só separador

            // Divide por || e espaços para isolar os nomes
            var partes = textoLimpo.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var parte in partes)
            {
                // Agora a parte pode ter múltiplos nomes ligados por &&
                var nomes = parte.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var nome in nomes)
                {
                    var nomeTrim = nome.Trim();
                    if (!string.IsNullOrEmpty(nomeTrim))
                    {
                        disciplinas.Add(nomeTrim);
                    }
                }
            }

            return disciplinas;
        }
    }
}
