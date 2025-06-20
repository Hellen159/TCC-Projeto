using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Data.Tests.Repository
{
    public class CursoRepositoryTests
    {
        /// <summary>
        /// Cria um novo contexto de banco de dados em memória para cada teste.
        /// O uso de um GUID como nome do banco garante total isolamento entre os testes.
        /// </summary>
        private DbContextOptions<MeuDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Testa se o método Adicionar retorna 'true' e persiste o curso no banco de dados quando os dados são válidos.
        /// </summary>
        [Fact]
        public async Task Adicionar_CursoValido_DeveRetornarTrueESalvarNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var novoCurso = new Curso
            {
                NomeCurso = "Ciência da Computação",
                CargaHorariaObrigatoria = 3200,
                CargaHorariaOptativa = 400
            };

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new CursoRepository(context);
                resultadoDaAcao = await repository.Adicionar(novoCurso);
            }

            // Assert
            Assert.True(resultadoDaAcao); // Verifica se o método Adicionar retornou true.

            using (var context = new MeuDbContext(options))
            {
                Assert.NotEqual(0, novoCurso.CodigoCurso); // Verifica se a chave primária foi gerada.
                var cursoSalvo = await context.Cursos.FindAsync(novoCurso.CodigoCurso);

                Assert.NotNull(cursoSalvo);
                Assert.Equal("Ciência da Computação", cursoSalvo.NomeCurso);
                Assert.Equal(3200, cursoSalvo.CargaHorariaObrigatoria);
            }
        }

        /// <summary>
        /// Testa se o método ObterPorId retorna o curso correto quando ele existe no banco.
        /// </summary>
        [Fact]
        public async Task ObterPorId_CursoExistente_DeveRetornarCursoCorreto()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var cursoExistente = new Curso { NomeCurso = "Curso Teste" };
            using (var context = new MeuDbContext(options))
            {
                context.Cursos.Add(cursoExistente);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new CursoRepository(context);
                var resultado = await repository.ObterPorId(cursoExistente.CodigoCurso);

                // Assert
                Assert.NotNull(resultado);
                Assert.Equal(cursoExistente.CodigoCurso, resultado.CodigoCurso);
                Assert.Equal("Curso Teste", resultado.NomeCurso);
            }
        }

        /// <summary>
        /// Testa se o método ObterPorId retorna nulo quando o curso não existe.
        /// </summary>
        [Fact]
        public async Task ObterPorId_CursoInexistente_DeveRetornarNull()
        {
            // Arrange
            var options = CreateNewContextOptions();

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new CursoRepository(context);
                var resultado = await repository.ObterPorId(999); // Um código que certamente não existe.

                // Assert
                Assert.Null(resultado);
            }
        }

        /// <summary>
        /// Testa se o método Atualizar modifica os dados de um curso existente no banco.
        /// </summary>
        [Fact]
        public async Task Atualizar_CursoExistente_DeveModificarDadosNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var cursoOriginal = new Curso { NomeCurso = "Nome Antigo", CargaHorariaObrigatoria = 1000 };
            using (var context = new MeuDbContext(options))
            {
                context.Cursos.Add(cursoOriginal);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new CursoRepository(context);
                var cursoParaAtualizar = await repository.ObterPorId(cursoOriginal.CodigoCurso);
                cursoParaAtualizar.NomeCurso = "Nome Novo";
                cursoParaAtualizar.CargaHorariaObrigatoria = 1500;

                await repository.Atualizar(cursoParaAtualizar);
            }

            // Assert
            using (var context = new MeuDbContext(options))
            {
                var cursoAtualizado = await context.Cursos.FindAsync(cursoOriginal.CodigoCurso);
                Assert.NotNull(cursoAtualizado);
                Assert.Equal("Nome Novo", cursoAtualizado.NomeCurso);
                Assert.Equal(1500, cursoAtualizado.CargaHorariaObrigatoria);
            }
        }

        /// <summary>
        /// Testa se o método Remover retorna 'true' e de fato remove o curso do banco.
        /// </summary>
        [Fact]
        public async Task Remover_CursoExistente_DeveRetornarTrueERemoverDoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var cursoParaRemover = new Curso { NomeCurso = "Curso a ser removido" };
            using (var context = new MeuDbContext(options))
            {
                context.Cursos.Add(cursoParaRemover);
                await context.SaveChangesAsync();
            }

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new CursoRepository(context);
                resultadoDaAcao = await repository.Remover(cursoParaRemover.CodigoCurso);
            }

            // Assert
            Assert.True(resultadoDaAcao); // Verifica se a ação de remover retornou true.

            using (var context = new MeuDbContext(options))
            {
                var resultado = await context.Cursos.FindAsync(cursoParaRemover.CodigoCurso);
                Assert.Null(resultado); // Garante que o curso não existe mais.
            }
        }

        /// <summary>
        /// Testa se o método Remover retorna 'false' ao tentar remover um curso que não existe.
        /// </summary>
        [Fact]
        public async Task Remover_CursoInexistente_DeveRetornarFalse()
        {
            // Arrange
            var options = CreateNewContextOptions();

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new CursoRepository(context);
                var resultadoDaAcao = await repository.Remover(999);

                // Assert
                Assert.False(resultadoDaAcao);
            }
        }
    }
}