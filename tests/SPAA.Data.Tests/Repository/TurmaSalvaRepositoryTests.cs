﻿// SPAA.Data.Tests.Repository/TurmaSalvaRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository; // Ensure your repository namespace is correct
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace SPAA.Data.Tests.Repository
{
    public class TurmaSalvaRepositoryTests
    {
        // Helper method to create new DbContextOptions for in-memory testing.
        private DbContextOptions<MeuDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        // --- Tests for inherited Adicionar method ---

        /// <summary>
        /// Tests if the Adicionar method saves a new saved class and returns true.
        /// The primary key (CodigoTurmaSalva, int) should be generated by the database.
        /// </summary>
        [Fact]
        public async Task Adicionar_TurmaSalvaValida_DeveRetornarTrueESalvarNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var novaTurmaSalva = new TurmaSalva
            {
                CodigoUnicoTurma = 101,
                Matricula = "MAT001",
                Horario = "SEG 08:00-10:00",
                CodigoDisciplina = "COMP101"
            };

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                resultadoDaAcao = await repository.Adicionar(novaTurmaSalva);
            }

            // Assert
            Assert.True(resultadoDaAcao);

            using (var context = new MeuDbContext(options))
            {
                var turmaSalva = await context.TurmasSalvas.FindAsync(novaTurmaSalva.CodigoTurmaSalva);
                Assert.NotNull(turmaSalva);
                Assert.NotEqual(0, turmaSalva.CodigoTurmaSalva);
                Assert.Equal(novaTurmaSalva.Matricula, turmaSalva.Matricula);
                Assert.Equal(novaTurmaSalva.Horario, turmaSalva.Horario);
            }
        }

        // --- Tests for inherited ObterPorId method ---

        /// <summary>
        /// Tests if the ObterPorId method returns the correct saved class for an existing key.
        /// </summary>
        [Fact]
        public async Task ObterPorId_ComChaveIntExistente_DeveRetornarTurmaSalvaCorreta()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var turmaSalvaExistente = new TurmaSalva
            {
                CodigoUnicoTurma = 102,
                Matricula = "MAT002",
                Horario = "TER 14:00-16:00",
                CodigoDisciplina = "COMP102"
            };
            int idGerado;

            using (var context = new MeuDbContext(options))
            {
                context.TurmasSalvas.Add(turmaSalvaExistente);
                await context.SaveChangesAsync();
                idGerado = turmaSalvaExistente.CodigoTurmaSalva;
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                var resultado = await repository.ObterPorId(idGerado);

                // Assert
                Assert.NotNull(resultado);
                Assert.Equal(idGerado, resultado.CodigoTurmaSalva);
                Assert.Equal(turmaSalvaExistente.Matricula, resultado.Matricula);
            }
        }

        /// <summary>
        /// Tests if the ObterPorId method returns null for a non-existent key.
        /// </summary>
        [Fact]
        public async Task ObterPorId_ComChaveIntInexistente_DeveRetornarNull()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var idInexistente = 999;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                var resultado = await repository.ObterPorId(idInexistente);

                // Assert
                Assert.Null(resultado);
            }
        }

        // --- Tests for inherited Atualizar method ---

        /// <summary>
        /// Tests if the Atualizar method modifies an existing saved class in the database.
        /// </summary>
        [Fact]
        public async Task Atualizar_TurmaSalvaExistente_DeveAtualizarNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var turmaSalvaOriginal = new TurmaSalva
            {
                CodigoUnicoTurma = 103,
                Matricula = "MAT003",
                Horario = "QUA 09:00-11:00",
                CodigoDisciplina = "COMP103"
            };
            int idParaAtualizar;

            using (var context = new MeuDbContext(options))
            {
                context.TurmasSalvas.Add(turmaSalvaOriginal);
                await context.SaveChangesAsync();
                idParaAtualizar = turmaSalvaOriginal.CodigoTurmaSalva;
            }

            // Modify the entity
            turmaSalvaOriginal.Horario = "QUA 13:00-15:00"; // Changed schedule

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                await repository.Atualizar(turmaSalvaOriginal);
            }

            // Assert
            using (var context = new MeuDbContext(options))
            {
                var turmaAtualizada = await context.TurmasSalvas.FindAsync(idParaAtualizar);
                Assert.NotNull(turmaAtualizada);
                Assert.Equal("QUA 13:00-15:00", turmaAtualizada.Horario);
            }
        }

        // --- Tests for inherited Remover method ---

        /// <summary>
        /// Tests if the Remover method removes an existing saved class and returns true.
        /// </summary>
        [Fact]
        public async Task Remover_TurmaSalvaExistente_DeveRetornarTrueERemoverDoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var turmaParaRemover = new TurmaSalva
            {
                CodigoUnicoTurma = 104,
                Matricula = "MAT004",
                Horario = "QUI 10:00-12:00",
                CodigoDisciplina = "COMP104"
            };
            int idParaRemover;

            using (var context = new MeuDbContext(options))
            {
                context.TurmasSalvas.Add(turmaParaRemover);
                await context.SaveChangesAsync();
                idParaRemover = turmaParaRemover.CodigoTurmaSalva;
            }

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                resultadoDaAcao = await repository.Remover(idParaRemover);
            }

            // Assert
            Assert.True(resultadoDaAcao);

            using (var context = new MeuDbContext(options))
            {
                var turmaRemovida = await context.TurmasSalvas.FindAsync(idParaRemover);
                Assert.Null(turmaRemovida);
            }
        }

        /// <summary>
        /// Tests if the Remover method returns false for a non-existent saved class.
        /// </summary>
        [Fact]
        public async Task Remover_TurmaSalvaInexistente_DeveRetornarFalse()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var idInexistente = 999;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                var resultadoDaAcao = await repository.Remover(idInexistente);

                // Assert
                Assert.False(resultadoDaAcao);
            }
        }

        // --- Tests for the new ExcluirTurmasSalvasDoAluno method ---

        /// <summary>
        /// Tests if ExcluirTurmasSalvasDoAluno removes all saved classes for a given student.
        /// </summary>
        [Fact]
        public async Task ExcluirTurmasSalvasDoAluno_AlunoComTurmas_DeveRemoverTodasETornarTrue()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var matriculaAluno1 = "ALUNO001";
            var matriculaAluno2 = "ALUNO002";

            using (var context = new MeuDbContext(options))
            {
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 201, Matricula = matriculaAluno1, Horario = "SEG 08:00", CodigoDisciplina = "DISC01" });
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 202, Matricula = matriculaAluno1, Horario = "SEG 10:00", CodigoDisciplina = "DISC02" });
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 203, Matricula = matriculaAluno2, Horario = "TER 09:00", CodigoDisciplina = "DISC03" }); // Different student
                await context.SaveChangesAsync();
            }

            // Act
            bool resultado;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                resultado = await repository.ExcluirTurmasSalvasDoAluno(matriculaAluno1);
            }

            // Assert
            Assert.True(resultado); // Should return true as classes were deleted
            using (var context = new MeuDbContext(options))
            {
                var turmasAluno1 = await context.TurmasSalvas.Where(ts => ts.Matricula == matriculaAluno1).ToListAsync();
                Assert.Empty(turmasAluno1); // Student 1 should have no classes left

                var turmasAluno2 = await context.TurmasSalvas.Where(ts => ts.Matricula == matriculaAluno2).ToListAsync();
                Assert.Single(turmasAluno2); // Student 2's classes should remain
            }
        }

        /// <summary>
        /// Tests if ExcluirTurmasSalvasDoAluno returns false when the student has no saved classes.
        /// </summary>
        [Fact]
        public async Task ExcluirTurmasSalvasDoAluno_AlunoSemTurmas_DeveRetornarFalse()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var matriculaInexistente = "ALUNO999";

            using (var context = new MeuDbContext(options))
            {
                // Add some data for other students, but not for ALUNO999
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 301, Matricula = "ALUNO003", Horario = "QUA 11:00", CodigoDisciplina = "DISC04" });
                await context.SaveChangesAsync();
            }

            // Act
            bool resultado;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                resultado = await repository.ExcluirTurmasSalvasDoAluno(matriculaInexistente);
            }

            // Assert
            Assert.False(resultado); // Should return false as no classes were deleted
            using (var context = new MeuDbContext(options))
            {
                Assert.Single(await context.TurmasSalvas.ToListAsync()); // Ensure other data is untouched
            }
        }

        /// <summary>
        /// Tests if HorariosComAulas returns an empty list when the student has no saved classes.
        /// </summary>
        [Fact]
        public async Task HorariosComAulas_AlunoSemTurmas_DeveRetornarListaVazia()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var matriculaInexistente = "ALUNO999";
            using (var context = new MeuDbContext(options))
            {
                // Ensure the database is not empty, but student 999 has no entries
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 501, Matricula = "ALUNO005", Horario = "SEX 08:00", CodigoDisciplina = "DISC10" });
                await context.SaveChangesAsync();
            }

            // Act
            List<string> horarios;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                horarios = await repository.HorariosComAulas(matriculaInexistente);
            }

            // Assert
            Assert.NotNull(horarios);
            Assert.Empty(horarios);
        }

        /// <summary>
        /// Tests if HorariosComAulas returns an empty list when the database is empty.
        /// </summary>
        [Fact]
        public async Task HorariosComAulas_BancoVazio_DeveRetornarListaVazia()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var matriculaQualquer = "ALUNO001"; // Doesn't matter, database is empty

            // Act
            List<string> horarios;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                horarios = await repository.HorariosComAulas(matriculaQualquer);
            }

            // Assert
            Assert.NotNull(horarios);
            Assert.Empty(horarios);
        }

        // --- Tests for the new TodasTurmasSalvasAluno method ---

        /// <summary>
        /// Tests if TodasTurmasSalvasAluno returns all saved classes for a specific student.
        /// </summary>
        [Fact]
        public async Task TodasTurmasSalvasAluno_AlunoComTurmas_DeveRetornarTodasAsTurmasDoAluno()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var matriculaAluno1 = "ALUNO600";
            var matriculaAluno2 = "ALUNO601";
            using (var context = new MeuDbContext(options))
            {
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 601, Matricula = matriculaAluno1, Horario = "SEG 08:00", CodigoDisciplina = "DISC11" });
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 602, Matricula = matriculaAluno1, Horario = "QUA 14:00", CodigoDisciplina = "DISC12" });
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 603, Matricula = matriculaAluno2, Horario = "TER 16:00", CodigoDisciplina = "DISC13" }); // Other student
                await context.SaveChangesAsync();
            }

            // Act
            List<TurmaSalva> turmasAluno;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                turmasAluno = await repository.TodasTurmasSalvasAluno(matriculaAluno1);
            }

            // Assert
            Assert.NotNull(turmasAluno);
            Assert.Equal(2, turmasAluno.Count);
            Assert.All(turmasAluno, ts => Assert.Equal(matriculaAluno1, ts.Matricula)); // All results belong to the correct student
            Assert.Contains(turmasAluno, ts => ts.CodigoUnicoTurma == 601);
            Assert.Contains(turmasAluno, ts => ts.CodigoUnicoTurma == 602);
        }

        /// <summary>
        /// Tests if TodasTurmasSalvasAluno returns an empty list when the student has no saved classes.
        /// </summary>
        [Fact]
        public async Task TodasTurmasSalvasAluno_AlunoSemTurmas_DeveRetornarListaVazia()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var matriculaInexistente = "ALUNO999";
            using (var context = new MeuDbContext(options))
            {
                context.TurmasSalvas.Add(new TurmaSalva { CodigoUnicoTurma = 701, Matricula = "ALUNO007", Horario = "QUA 08:00", CodigoDisciplina = "DISC14" });
                await context.SaveChangesAsync();
            }

            // Act
            List<TurmaSalva> turmasAluno;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                turmasAluno = await repository.TodasTurmasSalvasAluno(matriculaInexistente);
            }

            // Assert
            Assert.NotNull(turmasAluno);
            Assert.Empty(turmasAluno);
        }

        /// <summary>
        /// Tests if TodasTurmasSalvasAluno returns an empty list when the database is empty.
        /// </summary>
        [Fact]
        public async Task TodasTurmasSalvasAluno_BancoVazio_DeveRetornarListaVazia()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var matriculaQualquer = "ALUNO001";

            // Act
            List<TurmaSalva> turmasAluno;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TurmaSalvaRepository(context);
                turmasAluno = await repository.TodasTurmasSalvasAluno(matriculaQualquer);
            }

            // Assert
            Assert.NotNull(turmasAluno);
            Assert.Empty(turmasAluno);
        }
    }
}