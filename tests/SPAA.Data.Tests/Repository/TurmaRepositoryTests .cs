using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Data.Tests.Repository
{
    public class TurmaRepositoryTests : IDisposable
    {
        private readonly MeuDbContext _context;
        private readonly TurmaRepository _repo;

        public TurmaRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new MeuDbContext(options);
            _repo = new TurmaRepository(_context);
        }

        [Fact]
        public async Task Adicionar_DeveAdicionarTurma()
        {
            var turma = new Turma
            {
                CodigoTurma = "T001",
                NomeProfessor = "João",
                Capacidade = 30,
                Semestre = "2025-1",
                NomeDisciplina = "Matemática",
                Horario = "Seg 08:00-10:00",
                CodigoDisciplina = "MAT101"
            };

            var resultado = await _repo.Adicionar(turma);

            Assert.True(resultado);
            Assert.NotEqual(0, turma.CodigoTurmaUnico); // PK gerada pelo EF
            var turmaNoBanco = await _repo.ObterPorId(turma.CodigoTurmaUnico);
            Assert.NotNull(turmaNoBanco);
            Assert.Equal("João", turmaNoBanco.NomeProfessor);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarTurmaExistente()
        {
            var turma = new Turma
            {
                CodigoTurma = "T002",
                NomeProfessor = "Maria",
                Capacidade = 25,
                Semestre = "2025-1",
                NomeDisciplina = "Física",
                Horario = "Qua 10:00-12:00",
                CodigoDisciplina = "FIS101"
            };
            await _repo.Adicionar(turma);

            var obtida = await _repo.ObterPorId(turma.CodigoTurmaUnico);

            Assert.NotNull(obtida);
            Assert.Equal("Maria", obtida.NomeProfessor);
        }

        [Fact]
        public async Task Remover_DeveRemoverTurmaExistente()
        {
            var turma = new Turma
            {
                CodigoTurma = "T003",
                NomeProfessor = "Carlos",
                Capacidade = 20,
                Semestre = "2025-2",
                NomeDisciplina = "Química",
                Horario = "Sex 14:00-16:00",
                CodigoDisciplina = "QUI101"
            };
            await _repo.Adicionar(turma);

            var removido = await _repo.Remover(turma.CodigoTurmaUnico);

            Assert.True(removido);
            var obtida = await _repo.ObterPorId(turma.CodigoTurmaUnico);
            Assert.Null(obtida);
        }

        [Fact]
        public async Task Atualizar_DeveAtualizarTurmaExistente()
        {
            var turma = new Turma
            {
                CodigoTurma = "T004",
                NomeProfessor = "Ana",
                Capacidade = 40,
                Semestre = "2025-2",
                NomeDisciplina = "Biologia",
                Horario = "Ter 08:00-10:00",
                CodigoDisciplina = "BIO101"
            };
            await _repo.Adicionar(turma);

            turma.NomeProfessor = "Ana Atualizada";
            turma.Capacidade = 45;
            await _repo.Atualizar(turma);

            var atualizada = await _repo.ObterPorId(turma.CodigoTurmaUnico);
            Assert.Equal("Ana Atualizada", atualizada.NomeProfessor);
            Assert.Equal(45, atualizada.Capacidade);
        }

        [Fact]
        public async Task TurmasDisponiveisPorDisciplina_DeveRetornarTurmasCorretas()
        {
            var turma1 = new Turma
            {
                CodigoTurma = "T005",
                NomeProfessor = "Prof A",
                Capacidade = 30,
                Semestre = "2025-1",
                NomeDisciplina = "Matemática",
                Horario = "Seg 08:00-10:00",
                CodigoDisciplina = "MAT101"
            };
            var turma2 = new Turma
            {
                CodigoTurma = "T006",
                NomeProfessor = "Prof B",
                Capacidade = 25,
                Semestre = "2025-1",
                NomeDisciplina = "Matemática",
                Horario = "Qua 14:00-16:00",
                CodigoDisciplina = "MAT101"
            };
            var turma3 = new Turma
            {
                CodigoTurma = "T007",
                NomeProfessor = "Prof C",
                Capacidade = 20,
                Semestre = "2025-1",
                NomeDisciplina = "Física",
                Horario = "Sex 10:00-12:00",
                CodigoDisciplina = "FIS101"
            };

            await _repo.Adicionar(turma1);
            await _repo.Adicionar(turma2);
            await _repo.Adicionar(turma3);

            var turmasMatematica = await _repo.TurmasDisponiveisPorDisciplina("Matemática");

            Assert.NotNull(turmasMatematica);
            Assert.Equal(2, turmasMatematica.Count);
            Assert.All(turmasMatematica, t => Assert.Equal("Matemática", t.NomeDisciplina));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
