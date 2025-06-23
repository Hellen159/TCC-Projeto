// SPAA.Data.Tests.Repository/PreRequisitoRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository; // Certifique-se de que o namespace do seu repositório está correto
using System.Linq; // Necessário para Count()
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic; // Necessário para List

namespace SPAA.Data.Tests.Repository
{
    public class PreRequisitoRepositoryTests
    {
        // Método auxiliar para criar uma nova instância de DbContextOptions para testes em memória.
        private DbContextOptions<MeuDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        // --- Testes para o método Adicionar ---

        /// <summary>
        /// Testa se o método Adicionar salva um novo pré-requisito e retorna true.
        /// A chave primária (CodigoPreRequisito, int) deve ser gerada pelo banco.
        /// </summary>
        [Fact]
        public async Task Adicionar_PreRequisitoValido_DeveRetornarTrueESalvarNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var novoPreRequisito = new PreRequisito
            {
                NomeDisciplina = "Estruturas de Dados",
                PreRequisitoLogico = "COMP0101 AND MAT0102" // Exemplo de requisito lógico
            };

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new PreRequisitoRepository(context);
                resultadoDaAcao = await repository.Adicionar(novoPreRequisito);
            }

            // Assert
            Assert.True(resultadoDaAcao); // Verifica se a ação de adicionar foi bem-sucedida

            using (var context = new MeuDbContext(options))
            {
                // Como CodigoPreRequisito é gerada automaticamente, precisamos encontrar pelo ID preenchido
                var preRequisitoSalvo = await context.PreRequisitos.FindAsync(novoPreRequisito.CodigoPreRequisito);

                Assert.NotNull(preRequisitoSalvo);
                // Verifica se a chave primária foi gerada e não é o valor padrão (0)
                Assert.NotEqual(0, preRequisitoSalvo.CodigoPreRequisito);
                Assert.Equal(novoPreRequisito.NomeDisciplina, preRequisitoSalvo.NomeDisciplina);
                Assert.Equal(novoPreRequisito.PreRequisitoLogico, preRequisitoSalvo.PreRequisitoLogico);
            }
        }

        // --- Testes para o método ObterPorId ---

        /// <summary>
        /// Testa se o método ObterPorId retorna o pré-requisito correto para uma chave existente.
        /// A busca é feita pela chave primária INT (CodigoPreRequisito).
        /// </summary>
        [Fact]
        public async Task ObterPorId_ComChaveIntExistente_DeveRetornarPreRequisitoCorreto()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var preRequisitoExistente = new PreRequisito
            {
                NomeDisciplina = "Cálculo 1",
                PreRequisitoLogico = "SEM_PRE_REQUISITO"
            };
            int idGerado;

            using (var context = new MeuDbContext(options))
            {
                context.PreRequisitos.Add(preRequisitoExistente);
                await context.SaveChangesAsync();
                idGerado = preRequisitoExistente.CodigoPreRequisito; // Captura o ID gerado pelo banco
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new PreRequisitoRepository(context);
                var resultado = await repository.ObterPorId(idGerado);

                // Assert
                Assert.NotNull(resultado);
                Assert.Equal(idGerado, resultado.CodigoPreRequisito);
                Assert.Equal(preRequisitoExistente.NomeDisciplina, resultado.NomeDisciplina);
            }
        }

        /// <summary>
        /// Testa se o método ObterPorId retorna null para uma chave inexistente.
        /// </summary>
        [Fact]
        public async Task ObterPorId_ComChaveIntInexistente_DeveRetornarNull()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var idInexistente = 999; // Um ID que certamente não estará no banco vazio

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new PreRequisitoRepository(context);
                var resultado = await repository.ObterPorId(idInexistente);

                // Assert
                Assert.Null(resultado);
            }
        }

        // --- Testes para o método ObterTodos ---

        /// <summary>
        /// Testa se o método ObterTodos retorna todos os pré-requisitos.
        /// </summary>
        [Fact]
        public async Task ObterTodos_DeveRetornarTodosOsPreRequisitos()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new MeuDbContext(options))
            {
                context.PreRequisitos.Add(new PreRequisito { NomeDisciplina = "Algoritmos", PreRequisitoLogico = "NENHUM" });
                context.PreRequisitos.Add(new PreRequisito { NomeDisciplina = "Banco de Dados", PreRequisitoLogico = "EST0110" });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new PreRequisitoRepository(context);
                var resultados = await repository.ObterTodos(); // Assumindo que ObterTodos existe no seu Repository base

                // Assert
                Assert.NotNull(resultados);
                Assert.Equal(2, resultados.Count());
            }
        }

        // --- Testes para o método Remover ---

        /// <summary>
        /// Testa se o método Remover remove um pré-requisito existente e retorna true.
        /// A remoção é feita pela chave primária INT (CodigoPreRequisito).
        /// </summary>
        [Fact]
        public async Task Remover_PreRequisitoExistente_DeveRetornarTrueERemoverDoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var preRequisitoParaRemover = new PreRequisito
            {
                NomeDisciplina = "Programação Orientada a Objetos",
                PreRequisitoLogico = "COMP0401"
            };
            int idParaRemover;

            using (var context = new MeuDbContext(options))
            {
                context.PreRequisitos.Add(preRequisitoParaRemover);
                await context.SaveChangesAsync();
                idParaRemover = preRequisitoParaRemover.CodigoPreRequisito; // Obtém o ID gerado
            }

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new PreRequisitoRepository(context);
                resultadoDaAcao = await repository.Remover(idParaRemover);
            }

            // Assert
            Assert.True(resultadoDaAcao); // Verifica se a ação de remoção foi bem-sucedida

            using (var context = new MeuDbContext(options))
            {
                // Tenta encontrar o pré-requisito pelo ID para confirmar que foi removido
                var preRequisitoRemovido = await context.PreRequisitos.FindAsync(idParaRemover);
                Assert.Null(preRequisitoRemovido); // Espera que seja null
            }
        }

        /// <summary>
        /// Testa se o método Remover retorna false para um pré-requisito inexistente.
        /// </summary>
        [Fact]
        public async Task Remover_PreRequisitoInexistente_DeveRetornarFalse()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var idInexistente = 999;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new PreRequisitoRepository(context);
                var resultadoDaAcao = await repository.Remover(idInexistente);

                // Assert
                Assert.False(resultadoDaAcao); // Espera que a remoção falhe
            }
        }

        // --- Testes para o método Atualizar (considerando que é void/Task) ---

        /// <summary>
        /// Testa se o método Atualizar modifica um pré-requisito existente no banco de dados.
        /// (O método é void, então a verificação é feita consultando o banco após a chamada).
        /// </summary>
        [Fact]
        public async Task Atualizar_PreRequisitoExistente_DeveAtualizarNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var preRequisitoOriginal = new PreRequisito
            {
                NomeDisciplina = "Matemática Discreta",
                PreRequisitoLogico = "LOG001"
            };
            int idParaAtualizar;

            using (var context = new MeuDbContext(options))
            {
                context.PreRequisitos.Add(preRequisitoOriginal);
                await context.SaveChangesAsync();
                idParaAtualizar = preRequisitoOriginal.CodigoPreRequisito;
            }

            // Modifica o pré-requisito
            preRequisitoOriginal.NomeDisciplina = "Matemática Discreta (Atualizada)";
            preRequisitoOriginal.PreRequisitoLogico = "LOG001 OR LOG002";

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new PreRequisitoRepository(context);
                await repository.Atualizar(preRequisitoOriginal); // Chama o método void Atualizar
            }

            // Assert
            // Consulta o banco para verificar a atualização
            using (var context = new MeuDbContext(options))
            {
                var preRequisitoAtualizado = await context.PreRequisitos.FindAsync(idParaAtualizar);
                Assert.NotNull(preRequisitoAtualizado);
                Assert.Equal("Matemática Discreta (Atualizada)", preRequisitoAtualizado.NomeDisciplina);
                Assert.Equal("LOG001 OR LOG002", preRequisitoAtualizado.PreRequisitoLogico);
            }
        }
    }
}