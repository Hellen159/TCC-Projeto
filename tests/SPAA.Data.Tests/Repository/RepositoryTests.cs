// Crie esta classe em SPAA.Data.Tests.Repository/RepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models; // Ou SPAA.Data.Tests.Models
using SPAA.Data.Context;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

// Importe o namespace da sua TesteRepository
using SPAA.Data.Tests.Repository;

namespace SPAA.Data.Tests.Repository
{
    public class RepositoryTests
    {
        private DbContextOptions<MeuDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Atualizar_EntidadeNula_DeveLancarInvalidOperationException()
        {
            // Arrange
            var options = CreateNewContextOptions();
            TesteEntity entidadeNula = null;

            // Act & Assert
            using (var context = new MeuDbContext(options))
            {
                var repository = new TesteRepository(context);
                await Assert.ThrowsAsync<InvalidOperationException>(() => repository.Atualizar(entidadeNula));
            }
        }

        // --- COBRINDO AS LINHAS 66-68: Dispose ---
        /// <summary>
        /// Testa se o método Dispose é chamado e descarta o contexto.
        /// Tentar acessar o contexto após Dispose deve lançar uma ObjectDisposedException.
        /// </summary>
        [Fact]
        public void Dispose_DeveDescartarOContexto()
        {
            // Arrange
            var options = CreateNewContextOptions();
            MeuDbContext context = new MeuDbContext(options);
            var repository = new TesteRepository(context);

            // Act
            repository.Dispose();

            // Assert
            // Ao tentar acessar uma propriedade do contexto após o Dispose,
            // espera-se uma ObjectDisposedException.
            Assert.Throws<ObjectDisposedException>(() => context.TesteEntities.ToList());
        }


        // --- Testes para outros métodos (mantidos para completude) ---

        [Fact]
        public async Task Adicionar_EntidadeValida_DeveRetornarTrueESalvarNoBanco()
        {
            var options = CreateNewContextOptions();
            var novaEntidade = new TesteEntity { Nome = "Entidade de Teste 1", Descricao = "Descrição da Entidade 1" };
            bool resultadoDaAcao;

            using (var context = new MeuDbContext(options))
            {
                var repository = new TesteRepository(context);
                resultadoDaAcao = await repository.Adicionar(novaEntidade);
            }

            Assert.True(resultadoDaAcao);

            using (var context = new MeuDbContext(options))
            {
                var entidadeSalva = await context.TesteEntities.FindAsync(novaEntidade.CodigoTesteEntity);
                Assert.NotNull(entidadeSalva);
                Assert.NotEqual(0, entidadeSalva.CodigoTesteEntity);
                Assert.Equal(novaEntidade.Nome, entidadeSalva.Nome);
                Assert.Equal(novaEntidade.Descricao, entidadeSalva.Descricao);
            }
        }

        [Fact]
        public async Task ObterPorId_ComChaveExistente_DeveRetornarEntidadeCorreta()
        {
            var options = CreateNewContextOptions();
            var entidadeExistente = new TesteEntity { Nome = "Entidade Existente", Descricao = "Descrição da Entidade Existente" };
            int idGerado;

            using (var context = new MeuDbContext(options))
            {
                context.TesteEntities.Add(entidadeExistente);
                await context.SaveChangesAsync();
                idGerado = entidadeExistente.CodigoTesteEntity;
            }

            using (var context = new MeuDbContext(options))
            {
                var repository = new TesteRepository(context);
                var resultado = await repository.ObterPorId(idGerado);
                Assert.NotNull(resultado);
                Assert.Equal(idGerado, resultado.CodigoTesteEntity);
            }
        }

        [Fact]
        public async Task ObterPorId_ComChaveInexistente_DeveRetornarNull()
        {
            var options = CreateNewContextOptions();
            var idInexistente = 999;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TesteRepository(context);
                var resultado = await repository.ObterPorId(idInexistente);
                Assert.Null(resultado);
            }
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarTodasAsEntidades()
        {
            var options = CreateNewContextOptions();
            using (var context = new MeuDbContext(options))
            {
                context.TesteEntities.Add(new TesteEntity { Nome = "Entidade A", Descricao = "Desc A" });
                context.TesteEntities.Add(new TesteEntity { Nome = "Entidade B", Descricao = "Desc B" });
                await context.SaveChangesAsync();
            }

            using (var context = new MeuDbContext(options))
            {
                var repository = new TesteRepository(context);
                var resultados = await repository.ObterTodos();
                Assert.NotNull(resultados);
                Assert.Equal(2, resultados.Count());
            }
        }

        [Fact]
        public async Task Remover_EntidadeExistente_DeveRetornarTrueERemoverDoBanco()
        {
            var options = CreateNewContextOptions();
            var entidadeParaRemover = new TesteEntity { Nome = "Entidade para Remover", Descricao = "Descrição" };
            int idParaRemover;

            using (var context = new MeuDbContext(options))
            {
                context.TesteEntities.Add(entidadeParaRemover);
                await context.SaveChangesAsync();
                idParaRemover = entidadeParaRemover.CodigoTesteEntity;
            }

            bool resultadoDaAcao;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TesteRepository(context);
                resultadoDaAcao = await repository.Remover(idParaRemover);
            }

            Assert.True(resultadoDaAcao);

            using (var context = new MeuDbContext(options))
            {
                var entidadeRemovida = await context.TesteEntities.FindAsync(idParaRemover);
                Assert.Null(entidadeRemovida);
            }
        }

        [Fact]
        public async Task Remover_EntidadeInexistente_DeveRetornarFalse()
        {
            var options = CreateNewContextOptions();
            var idInexistente = 999;
            using (var context = new MeuDbContext(options))
            {
                var repository = new TesteRepository(context);
                var resultadoDaAcao = await repository.Remover(idInexistente);
                Assert.False(resultadoDaAcao);
            }
        }
    }
}