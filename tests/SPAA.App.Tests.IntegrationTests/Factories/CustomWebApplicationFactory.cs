// SPAA.App.Tests/Factories/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPAA.Data.Context; // Namespace correto do seu DbContext
using System;
using System.Linq;
using SPAA.Business.Models; // Adicionado para usar os modelos de negócio
using System.Collections.Generic;
using SPAA.Business.Interfaces.Services;
using SPAA.App.Tests.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; // <--- ADICIONE ESTE USING AQUI!

namespace SPAA.App.Tests.IntegrationTests.Factories
{
    // TProgram deve ser a sua classe Program (se .NET 6+) ou Startup (se .NET 5 ou anterior) do projeto SPAA.App
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove qualquer configuração de DbContext existente (a que aponta para o seu DB real)
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MeuDbContext>)); // Usando MeuDbContext

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Adiciona um DbContext configurado para usar um banco de dados in-memory para testes
                services.AddDbContext<MeuDbContext>(options => // Usando MeuDbContext
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting"); // Nome único para o banco de dados in-memory
                });

                // *** INÍCIO DA CONFIGURAÇÃO DO MOCK PARA IEmailService ***

                // Primeiro, remova qualquer registro existente para IEmailService.
                // Isso é importante caso o seu Program.cs já tenha registrado o EmailService real.
                var emailServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailService));

                if (emailServiceDescriptor != null)
                {
                    services.Remove(emailServiceDescriptor);
                }

                // Agora, adicione o seu MockEmailService como a implementação para IEmailService.
                // Usamos AddScoped, que é adequado para a maioria dos serviços em aplicativos web,
                // garantindo que uma nova instância seja criada por escopo (por requisição HTTP no teste).
                services.AddScoped<IEmailService, MockEmailService>();

                // *** FIM DA CONFIGURAÇÃO DO MOCK PARA IEmailService ***

                // *** INÍCIO DA CONFIGURAÇÃO PARA DESABILITAR VALIDATEANTIFORGERYTOKEN ***
                // Remove o filtro AutoValidateAntiforgeryTokenFilter para testes de integração.
                // Isso evita o erro 400 BadRequest quando não há token CSRF em requisições POST.
                services.Configure<MvcOptions>(options =>
                {
                    var antiForgeryFilter = options.Filters
                        .OfType<AutoValidateAntiforgeryTokenAttribute>()
                        .FirstOrDefault();
                    if (antiForgeryFilter != null)
                    {
                        options.Filters.Remove(antiForgeryFilter);
                    }
                    // Opcional: Adicionar um filtro que permite o anti-forgery, mas não o valida automaticamente
                    // options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
                });
                // *** FIM DA CONFIGURAÇÃO PARA DESABILITAR VALIDATEANTIFORGERYTOKEN ***


                // Cria um service provider temporário para obter o DbContext e popular dados de teste
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<MeuDbContext>(); // Obtém a instância do seu DbContext
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

                    // Garante que o banco de dados in-memory esteja limpo e recriado para cada execução da fábrica
                    db.Database.EnsureDeleted(); // Opcional, mas recomendado para isolar testes entre diferentes execuções de teste
                    db.Database.EnsureCreated(); // Cria o esquema do banco de dados in-memory

                    try
                    {
                        // --- Popule seu banco de dados in-memory com dados de teste AQUI ---
                        // Estes dados estarão disponíveis para todos os testes que usam esta fábrica.
                        // Adapte conforme os modelos e dados que seus testes de integração precisarão.
                        // Pense nos dados mínimos necessários para um cenário de sucesso e falha de um endpoint.

                        // Seeding para ApplicationUser (base para Aluno)
                        if (!db.Users.Any())
                        {
                            db.Users.Add(new ApplicationUser { Id = "user_test_001", UserName = "aluno1@test.com", Email = "aluno1@test.com", EmailConfirmed = true });
                            db.Users.Add(new ApplicationUser { Id = "user_test_002", UserName = "aluno2@test.com", Email = "aluno2@test.com", EmailConfirmed = true });
                            db.SaveChanges();
                        }

                        // Seeding para Alunos
                        if (!db.Alunos.Any())
                        {
                            db.Alunos.Add(new Aluno { Matricula = "202016481", NomeAluno = "Aluno de Teste 1", SemestreEntrada = "2020/1", HistoricoAnexado = true, CurriculoAluno = "2", CodigoUser = "user_test_001" });
                            db.Alunos.Add(new Aluno { Matricula = "202016482", NomeAluno = "Aluno de Teste 2", SemestreEntrada = "2020/1", HistoricoAnexado = true, CurriculoAluno = "2", CodigoUser = "user_test_002" });
                            db.SaveChanges();
                        }

                        // Seeding para TipoDisciplina
                        if (!db.TipoDisciplinas.Any())
                        {
                            db.TipoDisciplinas.Add(new TipoDisciplina { CodigoTipoDisiciplina = 1, NomeTipoDisciplina = "Obrigatoria" });
                            db.TipoDisciplinas.Add(new TipoDisciplina { CodigoTipoDisiciplina = 2, NomeTipoDisciplina = "Optativa" });
                            db.SaveChanges();
                        }

                        // Seeding para Curso
                        if (!db.Cursos.Any())
                        {
                            db.Cursos.Add(new Curso { CodigoCurso = 1, NomeCurso = "Ciencia da Computacao", CargaHorariaObrigatoria = 3000, CargaHorariaOptativa = 300 });
                            db.Cursos.Add(new Curso { CodigoCurso = 2, NomeCurso = "Engenharia de Software", CargaHorariaObrigatoria = 3200, CargaHorariaOptativa = 350 });
                            db.SaveChanges();
                        }

                        // Seeding para Disciplinas (Nomes ajustados sem acentos/caracteres especiais)
                        if (!db.Disciplinas.Any())
                        {
                            db.Disciplinas.Add(new Disciplina { CodigoDisciplina = 1, NomeDisciplina = "CALCULO 1", CargaHoraria = 90, Codigo = "01" });
                            db.Disciplinas.Add(new Disciplina { CodigoDisciplina = 2, NomeDisciplina = "PROGRAMACAO DE ALGORITMO", CargaHoraria = 90, Codigo = "02" });
                            db.Disciplinas.Add(new Disciplina { CodigoDisciplina = 3, NomeDisciplina = "BANCO DE DADOS", CargaHoraria = 60, Codigo = "03" });
                            db.Disciplinas.Add(new Disciplina { CodigoDisciplina = 4, NomeDisciplina = "MATEMATICA DISCRETA", CargaHoraria = 60, Codigo = "04" });
                            db.Disciplinas.Add(new Disciplina { CodigoDisciplina = 5, NomeDisciplina = "PROJETO INTEGRADOR 1", CargaHoraria = 60, Codigo = "05" }); // Optativa
                            db.Disciplinas.Add(new Disciplina { CodigoDisciplina = 6, NomeDisciplina = "LOGICA DIGITAL", CargaHoraria = 60, Codigo = "06" }); // Para pre-requisito
                            db.SaveChanges();
                        }

                        // Seeding para Turmas (NomeDisciplina e CodigoTurmaUnico ajustados)
                        if (!db.Turmas.Any())
                        {
                            // CALCULO 1
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 101, CodigoTurma = "C1M1", NomeProfessor = "Prof. X", Capacidade = 30, Semestre = "2024/1", NomeDisciplina = "CALCULO 1", Horario = "2M1,3M2", CodigoDisciplina = "01" });
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 102, CodigoTurma = "C1T1", NomeProfessor = "Prof. Y", Capacidade = 25, Semestre = "2024/1", NomeDisciplina = "CALCULO 1", Horario = "4T3,5T4", CodigoDisciplina = "01" });

                            // PROGRAMACAO DE ALGORITMO
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 201, CodigoTurma = "PAV1", NomeProfessor = "Prof. A", Capacidade = 35, Semestre = "2024/1", NomeDisciplina = "PROGRAMACAO DE ALGORITMO", Horario = "2V5,3V6", CodigoDisciplina = "02" });
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 202, CodigoTurma = "PBN1", NomeProfessor = "Prof. B", Capacidade = 30, Semestre = "2024/1", NomeDisciplina = "PROGRAMACAO DE ALGORITMO", Horario = "4N7,5N8", CodigoDisciplina = "02" });

                            // BANCO DE DADOS
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 301, CodigoTurma = "BDM1", NomeProfessor = "Prof. C", Capacidade = 20, Semestre = "2024/1", NomeDisciplina = "BANCO DE DADOS", Horario = "2M3,3M4", CodigoDisciplina = "03" });

                            // MATEMATICA DISCRETA
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 401, CodigoTurma = "MDV1", NomeProfessor = "Prof. D", Capacidade = 28, Semestre = "2024/1", NomeDisciplina = "MATEMATICA DISCRETA", Horario = "3V1,4V2", CodigoDisciplina = "04" });

                            // PROJETO INTEGRADOR 1 (Optativa)
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 501, CodigoTurma = "PI1N1", NomeProfessor = "Prof. E", Capacidade = 15, Semestre = "2024/1", NomeDisciplina = "PROJETO INTEGRADOR 1", Horario = "5N5", CodigoDisciplina = "05" });

                            // LOGICA DIGITAL (para pre-requisito)
                            db.Turmas.Add(new Turma { CodigoTurmaUnico = 601, CodigoTurma = "LDM1", NomeProfessor = "Prof. F", Capacidade = 20, Semestre = "2024/1", NomeDisciplina = "LOGICA DIGITAL", Horario = "2M1,3M2", CodigoDisciplina = "06" });

                            db.SaveChanges();
                        }

                        if (!db.PreRequisitos.Any())
                        {
                            db.PreRequisitos.Add(new PreRequisito { CodigoPreRequisito = 1, NomeDisciplina = "PROGRAMACAO DE ALGORITMO", PreRequisitoLogico = "CALCULO 1" });
                            db.PreRequisitos.Add(new PreRequisito { CodigoPreRequisito = 2, NomeDisciplina = "BANCO DE DADOS", PreRequisitoLogico = "PROGRAMACAO DE ALGORITMO AND LOGICA DIGITAL" });
                            db.PreRequisitos.Add(new PreRequisito { CodigoPreRequisito = 3, NomeDisciplina = "MATEMATICA DISCRETA", PreRequisitoLogico = "CALCULO 1" });
                            db.SaveChanges();
                        }

                        if (!db.AlunoDisciplinas.Any())
                        {
                            db.AlunoDisciplinas.Add(new AlunoDisciplina { Matricula = "202016481", NomeDisicplina = "CALCULO 1", Situacao = "APR", Semestre = "2023/2" });
                            db.AlunoDisciplinas.Add(new AlunoDisciplina { Matricula = "202016481", NomeDisicplina = "LOGICA DIGITAL", Situacao = "APR", Semestre = "2023/2" });
                            db.AlunoDisciplinas.Add(new AlunoDisciplina { Matricula = "202016481", NomeDisicplina = "PROGRAMACAO DE ALGORITMO", Situacao = "PEND", Semestre = "2024/1" });
                            db.AlunoDisciplinas.Add(new AlunoDisciplina { Matricula = "202016481", NomeDisicplina = "BANCO DE DADOS", Situacao = "PEND", Semestre = "2024/1" });
                            db.AlunoDisciplinas.Add(new AlunoDisciplina { Matricula = "202016482", NomeDisicplina = "CALCULO 1", Situacao = "PEND", Semestre = "2024/1" });
                            db.SaveChanges();
                        }

                        if (!db.TarefasAlunos.Any())
                        {
                            db.TarefasAlunos.Add(new TarefaAluno { CodigoTarefaAluno = 1, NomeTarefa = "Revisao Calculo", Matricula = "202016481", Horario = "2M1" });
                            db.SaveChanges();
                        }

                        if (!db.TurmasSalvas.Any())
                        {
                            db.TurmasSalvas.Add(new TurmaSalva { CodigoTurmaSalva = 1, CodigoUnicoTurma = 101, Matricula = "202016481", Horario = "2M1,3M2", CodigoDisciplina = "01" });
                            db.SaveChanges();
                        }

                        if (!db.Notificacoes.Any())
                        {
                            db.Notificacoes.Add(new Notificacao { CodigoNotificacao = 1, StatusNotificacao = 0, Mensagem = "Bem-vindo ao SPAA!" });
                            db.SaveChanges();
                        }

                        if (!db.TesteEntities.Any())
                        {
                            db.TesteEntities.Add(new TesteEntity { CodigoTesteEntity = 1, Descricao = "Dados de Teste 1" });
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ocorreu um erro ao semear o banco de dados com dados de teste. Erro: {Message}", ex.Message);
                    }
                }
            });

            builder.UseEnvironment("Development");
        }
    }
}
