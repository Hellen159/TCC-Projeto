using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SPAA.Business.Models;
using SPAA.Data.Context;

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
        string filePath = @"matrizCurricular2017.1.txt";

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(line)) continue;

                    string[] parts = line.Split('|');

                    if (parts.Length == 4)
                    {
                        string codigo = parts[0].Trim();
                        string nome = parts[1].Trim();
                        string cargaHoraria = parts[2].Trim();
                        string natureza = parts[3].Trim();

                        if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cargaHoraria) || string.IsNullOrEmpty(natureza))
                        {
                            Console.WriteLine($"Valores ausentes na linha: {line}");
                            continue;
                        }

                        int codigoTipoDisciplina = natureza.Equals("OBRIGATORIO", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

                        if (!int.TryParse(cargaHoraria.Replace("h", "").Trim(), out int cargaHorariaInt))
                        {
                            Console.WriteLine($"Carga horária inválida na linha: {line}");
                            continue;
                        }

                        var disciplina = new Disciplina
                        {
                            CodigoDisciplina = codigo,
                            NomeDisciplina = nome,
                            CargaHoraria = cargaHorariaInt,
                            CodigoTipoDisciplina = codigoTipoDisciplina,
                            CodigoCurso = 2, // Ajuste conforme necessário
                            Curriculo = "2017.1"
                        };

                        await context.Disciplinas.AddAsync(disciplina);
                        Console.WriteLine($"Disciplina {codigo} adicionada.");
                    }
                    else
                    {
                        Console.WriteLine($"Formato inválido na linha: {line}");
                    }
                }
            }

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
