using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository; // Certifique-se de que o namespace do seu repositório está correto
using System.Linq; // Necessário para FirstOrDefaultAsync e Count()
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic; // Necessário para List<Notificacao>

namespace SPAA.Data.Tests.Repository
{
    public class NotificacaoRepositoryTests
    {
        private DbContextOptions<MeuDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Testa se o método Adicionar salva uma nova notificação e retorna true.
        /// A chave primária (CodigoNotificacao, int) deve ser gerada pelo banco.
        /// </summary>
        [Fact]
        public async Task Adicionar_NotificacaoValida_DeveRetornarTrueESalvarNoBanco()
        {
            var options = CreateNewContextOptions();
            var novaNotificacao = new Notificacao
            {
                StatusNotificacao = 1,
                Mensagem = "Sua nova disciplina foi cadastrada com sucesso!"
            };

            bool resultadoDaAcao;

            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                resultadoDaAcao = await repository.Adicionar(novaNotificacao);
            }

            Assert.True(resultadoDaAcao);

            using (var context = new MeuDbContext(options))
            {
                var notificacaoSalva = await context.Notificacoes.FindAsync(novaNotificacao.CodigoNotificacao);

                Assert.NotNull(notificacaoSalva);
                Assert.NotEqual(0, notificacaoSalva.CodigoNotificacao);
                Assert.Equal(novaNotificacao.StatusNotificacao, notificacaoSalva.StatusNotificacao);
                Assert.Equal(novaNotificacao.Mensagem, notificacaoSalva.Mensagem);
            }
        }

        /// <summary>
        /// Testa se o método ObterPorId retorna a notificação correta para uma chave existente.
        /// A busca é feita pela chave primária INT (CodigoNotificacao).
        /// </summary>
        [Fact]
        public async Task ObterPorId_ComChaveIntExistente_DeveRetornarNotificacaoCorreta()
        {
            var options = CreateNewContextOptions();
            var notificacaoExistente = new Notificacao
            {
                StatusNotificacao = 1,
                Mensagem = "Notificação de teste para busca por ID."
            };
            int idGerado;

            using (var context = new MeuDbContext(options))
            {
                context.Notificacoes.Add(notificacaoExistente);
                await context.SaveChangesAsync();
                idGerado = notificacaoExistente.CodigoNotificacao;
            }

            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                var resultado = await repository.ObterPorId(idGerado);

                Assert.NotNull(resultado);
                Assert.Equal(idGerado, resultado.CodigoNotificacao);
                Assert.Equal(notificacaoExistente.Mensagem, resultado.Mensagem);
            }
        }

        /// <summary>
        /// Testa se o método ObterPorId retorna null para uma chave inexistente.
        /// </summary>
        [Fact]
        public async Task ObterPorId_ComChaveIntInexistente_DeveRetornarNull()
        {
            var options = CreateNewContextOptions();
            var idInexistente = 999;

            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                var resultado = await repository.ObterPorId(idInexistente);

                Assert.Null(resultado);
            }
        }

        /// <summary>
        /// Testa se o método ObterTodos retorna todas as notificações.
        /// </summary>
        [Fact]
        public async Task ObterTodos_DeveRetornarTodasAsNotificacoes()
        {
            var options = CreateNewContextOptions();
            using (var context = new MeuDbContext(options))
            {
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 1, Mensagem = "Msg 1" });
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 0, Mensagem = "Msg 2" });
                await context.SaveChangesAsync();
            }

            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                var resultados = await repository.ObterTodos();

                Assert.NotNull(resultados);
                Assert.Equal(2, resultados.Count());
            }
        }

        /// <summary>
        /// Testa se o método Remover remove uma notificação existente e retorna true.
        /// A remoção é feita pela chave primária INT (CodigoNotificacao).
        /// </summary>
        [Fact]
        public async Task Remover_NotificacaoExistente_DeveRetornarTrueERemoverDoBanco()
        {
            var options = CreateNewContextOptions();
            var notificacaoParaRemover = new Notificacao
            {
                StatusNotificacao = 0,
                Mensagem = "Notificação a ser removida."
            };
            int idParaRemover;

            using (var context = new MeuDbContext(options))
            {
                context.Notificacoes.Add(notificacaoParaRemover);
                await context.SaveChangesAsync();
                idParaRemover = notificacaoParaRemover.CodigoNotificacao;
            }

            bool resultadoDaAcao;

            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                resultadoDaAcao = await repository.Remover(idParaRemover);
            }

            Assert.True(resultadoDaAcao);

            using (var context = new MeuDbContext(options))
            {
                var notificacaoRemovida = await context.Notificacoes.FindAsync(idParaRemover);
                Assert.Null(notificacaoRemovida);
            }
        }

        /// <summary>
        /// Testa se o método Remover retorna false para uma notificação inexistente.
        /// </summary>
        [Fact]
        public async Task Remover_NotificacaoInexistente_DeveRetornarFalse()
        {
            var options = CreateNewContextOptions();
            var idInexistente = 999;

            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                var resultadoDaAcao = await repository.Remover(idInexistente);

                Assert.False(resultadoDaAcao);
            }
        }


        /// <summary>
        /// Testa se o método Atualizar modifica uma notificação existente no banco de dados.
        /// (O método é void, então a verificação é feita consultando o banco após a chamada).
        /// </summary>
        [Fact]
        public async Task Atualizar_NotificacaoExistente_DeveAtualizarNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var notificacaoOriginal = new Notificacao
            {
                StatusNotificacao = 1,
                Mensagem = "Mensagem original."
            };
            int idParaAtualizar;

            using (var context = new MeuDbContext(options))
            {
                context.Notificacoes.Add(notificacaoOriginal);
                await context.SaveChangesAsync(); // Salva a original para obter o ID gerado
                idParaAtualizar = notificacaoOriginal.CodigoNotificacao;
            }

            // Modifica a notificação que será passada para o método de atualização
            // (IMPORTANTE: A mesma instância que foi "trackeada" ou uma nova com o mesmo ID PK)
            // No Entity Framework Core, para o método Update funcionar bem, a entidade deve
            // estar sendo rastreada ou você deve anexá-la antes de chamar Update,
            // ou recarregá-la e modificar. No ambiente de teste em memória, muitas vezes
            // a mesma instância pode ser modificada e passada.
            notificacaoOriginal.StatusNotificacao = 0; // Ex: mudou para "lida"
            notificacaoOriginal.Mensagem = "Mensagem atualizada.";

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                // Chama o método Atualizar (que agora é void/Task)
                await repository.Atualizar(notificacaoOriginal);
                // Após o método Atualizar (que deve ter um SaveChanges() implícito ou explícito),
                // os dados no banco em memória devem estar atualizados.
            }

            // Assert
            // Agora, consultamos o banco de dados para verificar se a atualização realmente ocorreu
            using (var context = new MeuDbContext(options))
            {
                var notificacaoAtualizada = await context.Notificacoes.FindAsync(idParaAtualizar);
                Assert.NotNull(notificacaoAtualizada);
                Assert.Equal(0, notificacaoAtualizada.StatusNotificacao); // Verifica o novo status
                Assert.Equal("Mensagem atualizada.", notificacaoAtualizada.Mensagem); // Verifica a nova mensagem
            }
        }

        // --- Testes para o novo método ObterNotificacaoPorStatus ---

        /// <summary>
        /// Testa o método ObterNotificacaoPorStatus para retornar notificações com um status específico.
        /// </summary>
        [Fact]
        public async Task ObterNotificacaoPorStatus_DeveRetornarNotificacoesComStatusCorreto()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var statusEsperado = 1; // Ex: status "lida"
            using (var context = new MeuDbContext(options))
            {
                // Adiciona algumas notificações com status diferentes
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 1, Mensagem = "Notificação Lida 1" });
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 0, Mensagem = "Notificação Não Lida 1" });
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 1, Mensagem = "Notificação Lida 2" });
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 0, Mensagem = "Notificação Não Lida 2" });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                var resultados = await repository.ObterNotificacaoPorStatus(statusEsperado);

                // Assert
                Assert.NotNull(resultados);
                // Espera que apenas 2 notificações tenham o status 1
                Assert.Equal(2, resultados.Count);
                // Garante que todas as notificações retornadas têm o status correto
                Assert.All(resultados, n => Assert.Equal(statusEsperado, n.StatusNotificacao));
            }
        }

        /// <summary>
        /// Testa o método ObterNotificacaoPorStatus quando não há notificações para o status procurado.
        /// Deve retornar uma lista vazia.
        /// </summary>
        [Fact]
        public async Task ObterNotificacaoPorStatus_ComStatusInexistente_DeveRetornarListaVazia()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var statusInexistente = 99; // Um status que não existe nas notificações
            using (var context = new MeuDbContext(options))
            {
                // Adiciona algumas notificações, mas nenhuma com o status 99
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 0, Mensagem = "Notificação A" });
                context.Notificacoes.Add(new Notificacao { StatusNotificacao = 1, Mensagem = "Notificação B" });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                var resultados = await repository.ObterNotificacaoPorStatus(statusInexistente);

                // Assert
                Assert.NotNull(resultados);
                Assert.Empty(resultados); // Espera uma lista vazia
            }
        }

        /// <summary>
        /// Testa o método ObterNotificacaoPorStatus em um banco de dados vazio.
        /// Deve retornar uma lista vazia.
        /// </summary>
        [Fact]
        public async Task ObterNotificacaoPorStatus_EmBancoVazio_DeveRetornarListaVazia()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var statusQualquer = 1;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new NotificacaoRepository(context);
                var resultados = await repository.ObterNotificacaoPorStatus(statusQualquer);

                // Assert
                Assert.NotNull(resultados);
                Assert.Empty(resultados); // Espera uma lista vazia
            }
        }
    }
}