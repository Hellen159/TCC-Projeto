using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq; // Adicionar para usar .All() e .Count()
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
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Garante um banco de dados em memória único para cada execução de teste
                .Options;

            _context = new MeuDbContext(options);
            _repo = new TurmaRepository(_context);
        }

        [Fact]
        public async Task Adicionar_DeveAdicionarTurma()
        {
            // Arrange
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

            // Act
            var resultado = await _repo.Adicionar(turma);

            // Assert
            Assert.True(resultado);
            Assert.NotEqual(0, turma.CodigoTurmaUnico); // PK gerada pelo EF
            var turmaNoBanco = await _repo.ObterPorId(turma.CodigoTurmaUnico);
            Assert.NotNull(turmaNoBanco);
            Assert.Equal("João", turmaNoBanco.NomeProfessor);
            Assert.Equal("MAT101", turmaNoBanco.CodigoDisciplina);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarTurmaExistente()
        {
            // Arrange
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

            // Act
            var obtida = await _repo.ObterPorId(turma.CodigoTurmaUnico);

            // Assert
            Assert.NotNull(obtida);
            Assert.Equal("Maria", obtida.NomeProfessor);
            Assert.Equal("FIS101", obtida.CodigoDisciplina);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNullParaTurmaInexistente()
        {
            // Arrange
            var idInexistente = 999; // Um ID que não existe no banco

            // Act
            var obtida = await _repo.ObterPorId(idInexistente);

            // Assert
            Assert.Null(obtida);
        }

        [Fact]
        public async Task Remover_DeveRemoverTurmaExistente()
        {
            // Arrange
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

            // Act
            var removido = await _repo.Remover(turma.CodigoTurmaUnico);

            // Assert
            Assert.True(removido);
            var obtida = await _repo.ObterPorId(turma.CodigoTurmaUnico);
            Assert.Null(obtida); // Deve ser nulo após a remoção
        }

        [Fact]
        public async Task Remover_DeveRetornarFalseParaTurmaInexistente()
        {
            // Arrange
            var idInexistente = 999;

            // Act
            var removido = await _repo.Remover(idInexistente);

            // Assert
            Assert.False(removido); // A remoção de algo que não existe deve retornar false
        }

        [Fact]
        public async Task Atualizar_DeveAtualizarTurmaExistente()
        {
            // Arrange
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

            // Modifica a turma
            turma.NomeProfessor = "Ana Atualizada";
            turma.Capacidade = 45;
            turma.Horario = "Ter 09:00-11:00";

            // Act
            await _repo.Atualizar(turma);

            // Assert
            var atualizada = await _repo.ObterPorId(turma.CodigoTurmaUnico);
            Assert.NotNull(atualizada);
            Assert.Equal("Ana Atualizada", atualizada.NomeProfessor);
            Assert.Equal(45, atualizada.Capacidade);
            Assert.Equal("Ter 09:00-11:00", atualizada.Horario);
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarTodasAsTurmas()
        {
            // Arrange
            await _repo.Adicionar(new Turma { CodigoTurma = "T008", NomeDisciplina = "História", Semestre = "2025-1", CodigoDisciplina = "HIS101", Horario = "Seg 14:00-16:00", NomeProfessor = "Pedro", Capacidade = 30 });
            await _repo.Adicionar(new Turma { CodigoTurma = "T009", NomeDisciplina = "Geografia", Semestre = "2025-1", CodigoDisciplina = "GEO101", Horario = "Ter 16:00-18:00", NomeProfessor = "Lucas", Capacidade = 25 });

            // Act
            var todasAsTurmas = await _repo.ObterTodos();

            // Assert
            Assert.NotNull(todasAsTurmas);
            Assert.Equal(2, todasAsTurmas.Count()); // Assumindo que apenas 2 turmas foram adicionadas
        }

        [Fact]
        public async Task TurmasDisponiveisPorDisciplina_DeveRetornarTurmasCorretas()
        {
            // Arrange
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
                NomeDisciplina = "Física", // Disciplina diferente
                Horario = "Sex 10:00-12:00",
                CodigoDisciplina = "FIS101"
            };

            await _repo.Adicionar(turma1);
            await _repo.Adicionar(turma2);
            await _repo.Adicionar(turma3);

            // Act
            var turmasMatematica = await _repo.TurmasDisponiveisPorDisciplina("Matemática");

            // Assert
            Assert.NotNull(turmasMatematica);
            Assert.Equal(2, turmasMatematica.Count);
            // Verifica se todas as turmas retornadas são da disciplina "Matemática"
            Assert.All(turmasMatematica, t => Assert.Equal("Matemática", t.NomeDisciplina));
            // Verifica se as turmas corretas estão presentes
            Assert.Contains(turmasMatematica, t => t.CodigoTurma == "T005");
            Assert.Contains(turmasMatematica, t => t.CodigoTurma == "T006");
            Assert.DoesNotContain(turmasMatematica, t => t.CodigoTurma == "T007"); // Não deve conter a turma de Física
        }

        [Fact]
        public async Task TurmasDisponiveisPorDisciplina_DeveRetornarListaVaziaParaDisciplinaInexistente()
        {
            // Arrange
            await _repo.Adicionar(new Turma { NomeDisciplina = "Matemática", Semestre = "2025-1", CodigoTurma = "T010", Horario = "Seg 08:00-10:00", NomeProfessor = "Prof D", Capacidade = 20, CodigoDisciplina = "MAT101" });

            // Act
            var turmasInexistentes = await _repo.TurmasDisponiveisPorDisciplina("Programação");

            // Assert
            Assert.NotNull(turmasInexistentes);
            Assert.Empty(turmasInexistentes); // Deve retornar uma lista vazia
        }

        // --- Novos testes para TurmasDisponiveisPorSemestre ---

        [Fact]
        public async Task TurmasDisponiveisPorSemestre_DeveRetornarTurmasCorretas()
        {
            // Arrange
            var turmaA = new Turma { CodigoTurma = "S001", NomeDisciplina = "Cálculo I", Semestre = "2025-1", CodigoDisciplina = "CAL101", Horario = "Seg 08:00", NomeProfessor = "Paula", Capacidade = 50 };
            var turmaB = new Turma { CodigoTurma = "S002", NomeDisciplina = "Álgebra Linear", Semestre = "2025-1", CodigoDisciplina = "ALG101", Horario = "Ter 10:00", NomeProfessor = "Ricardo", Capacidade = 45 };
            var turmaC = new Turma { CodigoTurma = "S003", NomeDisciplina = "Cálculo II", Semestre = "2025-2", CodigoDisciplina = "CAL201", Horario = "Qua 14:00", NomeProfessor = "Fernanda", Capacidade = 40 };

            await _repo.Adicionar(turmaA);
            await _repo.Adicionar(turmaB);
            await _repo.Adicionar(turmaC);

            // Act
            var turmasSemestre1 = await _repo.TurmasDisponiveisPorSemestre("2025-1");

            // Assert
            Assert.NotNull(turmasSemestre1);
            Assert.Equal(2, turmasSemestre1.Count);
            // Verifica se todas as turmas retornadas são do semestre "2025-1"
            Assert.All(turmasSemestre1, t => Assert.Equal("2025-1", t.Semestre));
            Assert.Contains(turmasSemestre1, t => t.CodigoTurma == "S001");
            Assert.Contains(turmasSemestre1, t => t.CodigoTurma == "S002");
            Assert.DoesNotContain(turmasSemestre1, t => t.CodigoTurma == "S003"); // Não deve conter a turma de 2025-2
        }

        [Fact]
        public async Task TurmasDisponiveisPorSemestre_DeveRetornarListaVaziaParaSemestreInexistente()
        {
            // Arrange
            await _repo.Adicionar(new Turma { CodigoTurma = "S004", NomeDisciplina = "Lógica", Semestre = "2025-1", CodigoDisciplina = "LOG101", Horario = "Qui 09:00", NomeProfessor = "Mariana", Capacidade = 35 });

            // Act
            var turmasSemestreInexistente = await _repo.TurmasDisponiveisPorSemestre("2026-1");

            // Assert
            Assert.NotNull(turmasSemestreInexistente);
            Assert.Empty(turmasSemestreInexistente); // Deve retornar uma lista vazia
        }

        [Fact]
        public async Task TurmasDisponiveisPorSemestre_BancoDeDadosVazio_DeveRetornarListaVazia()
        {
            // Arrange (o banco de dados já começa vazio devido ao Dispose e nova instância no construtor)
            var semestre = "2025-1";

            // Act
            var turmas = await _repo.TurmasDisponiveisPorSemestre(semestre);

            // Assert
            Assert.NotNull(turmas);
            Assert.Empty(turmas);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}