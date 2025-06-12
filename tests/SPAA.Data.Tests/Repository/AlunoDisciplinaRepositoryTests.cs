using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Data.Tests.Repository
{
    public class AlunoDisciplinaRepositoryTests
    {
        private readonly MeuDbContext _context;
        private readonly AlunoDisciplinaRepository _repository;

        public AlunoDisciplinaRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // banco isolado por teste
                .Options;

            _context = new MeuDbContext(options);

            var loggerMock = new Mock<ILogger<AlunoDisciplinaRepository>>();
            var alunoRepoMock = new Mock<IAlunoRepository>();
            var alunoServiceMock = new Mock<IAlunoService>();

            _repository = new AlunoDisciplinaRepository(_context,
                                                         loggerMock.Object,
                                                         alunoRepoMock.Object,
                                                         alunoServiceMock.Object);
        }

        [Fact]
        public async Task ExcluirDisciplinasDoAluno_DeveRemoverDisciplinasERetornarTrue()
        {
            var matricula = "123";
            _context.AlunoDisciplinas.AddRange(new[]
            {
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "Algoritmos", Situacao = "APR", Semestre = "2025.1" },
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "POO", Situacao = "APR", Semestre = "2025.1" }
            });
            await _context.SaveChangesAsync();

            var resultado = await _repository.ExcluirDisciplinasDoAluno(matricula);

            Assert.True(resultado);
            Assert.Empty(await _context.AlunoDisciplinas.ToListAsync());
        }

        [Fact]
        public async Task ExcluirDisciplinasDoAluno_SemDisciplinas_DeveRetornarFalse()
        {
            var resultado = await _repository.ExcluirDisciplinasDoAluno("999");
            Assert.False(resultado);
        }

        [Fact]
        public async Task ObterAlunoDisciplinaPorSituacao_DeveRetornarCorretamente()
        {
            var matricula = "456";
            _context.AlunoDisciplinas.AddRange(new[]
            {
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "Matemática", Situacao = "APR", Semestre = "2025.1" },
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "História", Situacao = "REP", Semestre = "2025.1" }
            });
            await _context.SaveChangesAsync();

            var resultado = await _repository.ObterAlunoDisciplinaPorSituacao(matricula, "APR");

            Assert.Single(resultado);
            Assert.Equal("Matemática", resultado[0].NomeDisicplina);
        }

        [Fact]
        public async Task ObterNomeDisciplinasPorSituacao_DeveRetornarNomesUnicos()
        {
            var matricula = "789";
            _context.AlunoDisciplinas.AddRange(new[]
            {
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "Biologia", Situacao = "APR", Semestre = "2025.1" },
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "Biologia", Situacao = "APR", Semestre = "2025.2" }, // diferente semestre, chave composta diferente
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "Física", Situacao = "APR", Semestre = "2025.1" }
            });
            await _context.SaveChangesAsync();

            var nomes = await _repository.ObterNomeDisciplinasPorSituacao(matricula, "APR");

            Assert.Equal(2, nomes.Count); // nomes únicos, independente do semestre
            Assert.Contains("Biologia", nomes);
            Assert.Contains("Física", nomes);
        }

        [Fact]
        public async Task InserirDisciplinas_DeveInserirComSucesso()
        {
            var disciplinas = new List<AlunoDisciplina>
            {
                new AlunoDisciplina { Matricula = "999", NomeDisicplina = "Redes", Situacao = "APR", Semestre = "2025.1" },
                new AlunoDisciplina { Matricula = "999", NomeDisicplina = "Compiladores", Situacao = "APR", Semestre = "2025.1" }
            };

            await _repository.InserirDisciplinas(disciplinas);

            var resultado = await _context.AlunoDisciplinas.ToListAsync();
            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task InserirEquivalencias_DeveIgnorarDuplicados()
        {
            var matricula = "111";
            _context.AlunoDisciplinas.Add(new AlunoDisciplina
            {
                Matricula = matricula,
                NomeDisicplina = "IA",
                Situacao = "APR",
                Semestre = "2025.1"
            });
            await _context.SaveChangesAsync();

            var equivalencias = new List<AlunoDisciplina>
            {
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "IA", Situacao = "APR", Semestre = "2025.1" },  // já existe
                new AlunoDisciplina { Matricula = matricula, NomeDisicplina = "Engenharia de Software", Situacao = "APR", Semestre = "2025.1" }
            };

            await _repository.InserirEquivalencias(equivalencias, matricula);

            var resultado = await _context.AlunoDisciplinas
                .Where(a => a.Matricula == matricula)
                .ToListAsync();

            Assert.Equal(2, resultado.Count); // só adiciona a nova
            Assert.Contains(resultado, d => d.NomeDisicplina == "Engenharia de Software");
        }
    }
}
