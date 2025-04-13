using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SPAA.Data.Context;
using SPAA.Business.Models;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Cria o host com DI e configurações
        var host = CreateHostBuilder(args).Build();

        // Resolve o DbContext com escopo
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MeuDbContext>();

        // Caminho do arquivo TXT
        string filePath = @"teste.txt";

        try
        {
            string fileContent = File.ReadAllText(filePath);
            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string[] parts = line.Split('|');

                if (parts.Length == 5)
                {
                    string codigo = parts[0].Trim();
                    string nome = parts[1].Trim();
                    string cargaHoraria = parts[2].Trim();
                    string tipo = parts[3].Trim();
                    string natureza = parts[4].Trim();

                    // Validação e padronização
                    if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cargaHoraria) || string.IsNullOrEmpty(tipo) || string.IsNullOrEmpty(natureza))
                    {
                        Console.WriteLine($"Valores ausentes na linha: {line}");
                        continue;  // Ignora linhas com dados faltando
                    }

                    // Padronizando a natureza
                    int codigoTipoDisciplina = natureza.Equals("OBRIGATORIO", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

                    // Valida a carga horária e remove o sufixo "h" caso exista
                    if (!int.TryParse(cargaHoraria.Replace("h", "").Trim(), out int cargaHorariaInt))
                    {
                        Console.WriteLine($"Carga horária inválida na linha: {line}");
                        continue;
                    }

                    // Verifica se já existe a disciplina com o mesmo código
                    var existingDisciplina = await context.Disciplinas
                        .AsNoTracking() // Evita que o DbContext rastreie a entidade
                        .FirstOrDefaultAsync(d => d.CodigoDisciplina == codigo);

                    if (existingDisciplina != null)
                    {
                        // Se já existir, não adicione novamente
                        Console.WriteLine($"Disciplina com código {codigo} já existe.");
                    }
                    else
                    {
                        // Criação da nova disciplina
                        var disciplina = new Disciplina
                        {
                            CodigoDisciplina = codigo,
                            NomeDisciplina = nome,
                            CargaHoraria = cargaHorariaInt,
                            CodigoTipoDisciplina = codigoTipoDisciplina,
                            CodigoCurso = 2 // Ajuste conforme necessário
                        };

                        // Adiciona a disciplina ao contexto
                        await context.Disciplinas.AddAsync(disciplina);
                        Console.WriteLine($"Disciplina com código {codigo} adicionada.");
                    }
                }
                else
                {
                    Console.WriteLine($"Formato inválido na linha: {line}");
                }
            }

            // Salva todas as disciplinas de uma vez
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {ex.Message}");
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<MeuDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            });
}
