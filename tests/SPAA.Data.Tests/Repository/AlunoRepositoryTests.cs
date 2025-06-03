using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System;
using System.Threading.Tasks;
using Xunit;
namespace SPAA.Data.Tests.Repository
{
    public class AlunoRepositoryTests
    {
        private MeuDbContext ObterContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // banco isolado por teste
                .Options;

            var contexto = new MeuDbContext(options);
            return contexto;
        }

        [Fact]
        public async Task ObterIdentityUserIdPorMatricula_DeveRetornarIdDoUsuario_QuandoAlunoExistir()
        {
            // Arrange
            var contexto = ObterContextoEmMemoria();

            var aluno = new Aluno
            {
                Matricula = "123456789",
                NomeAluno = "Aluno Teste",          // agora obrigatório preenchido
                SemestreEntrada = "2025.1",          // agora obrigatório preenchido
                CodigoUser = "user-id-123",
                User = new ApplicationUser { Id = "user-id-123" }
            };

            contexto.Set<Aluno>().Add(aluno);
            await contexto.SaveChangesAsync();

            var repo = new AlunoRepository(contexto);

            // Act
            var userId = await repo.ObterIdentityUserIdPorMatricula("123456789");

            // Assert
            Assert.Equal("user-id-123", userId);
        }

        [Fact]
        public async Task ObterIdentityUserIdPorMatricula_DeveLancarExcecao_QuandoAlunoNaoExistir()
        {
            // Arrange
            var contexto = ObterContextoEmMemoria();
            var repo = new AlunoRepository(contexto);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => repo.ObterIdentityUserIdPorMatricula("matricula-invalida"));
        }

        [Fact]
        public async Task ObterIdentityUserIdPorMatricula_DeveLancarExcecao_QuandoMatriculaNula()
        {
            // Arrange
            var contexto = ObterContextoEmMemoria();
            var repo = new AlunoRepository(contexto);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => repo.ObterIdentityUserIdPorMatricula(null));
        }
    }
}