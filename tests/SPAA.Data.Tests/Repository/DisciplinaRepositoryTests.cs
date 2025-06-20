using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository; // Certifique-se de que o namespace do seu repositório está correto
using System.Linq; // Necessário para FirstOrDefaultAsync
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Data.Tests.Repository
{
    public class DisciplinaRepositoryTests
    {
        private DbContextOptions<MeuDbContext> CreateNewContextOptions()
        {
            // Cria um banco de dados em memória com um nome único para cada teste,
            // garantindo que os testes sejam isolados.
            return new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Testa se o método Adicionar salva uma nova disciplina e retorna true.
        /// A chave primária da entidade (CodigoDisciplina, do tipo int) é gerada pelo banco.
        /// </summary>
        [Fact]
        public async Task Adicionar_DisciplinaValida_DeveRetornarTrueESalvarNoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var novaDisciplina = new Disciplina
            {
                Codigo = "COMP0451", // Este é o código de negócio (string), NÃO a PK
                NomeDisciplina = "Inteligência Artificial",
                CargaHoraria = 64,
                Ementa = "Fundamentos de IA, busca, aprendizado de máquina."
            };

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new DisciplinaRepository(context);
                resultadoDaAcao = await repository.Adicionar(novaDisciplina);
            }

            // Assert
            // Primeiro, verifica se a ação de adicionar foi bem-sucedida
            Assert.True(resultadoDaAcao);

            // Agora, verifica se a disciplina foi realmente salva no banco
            using (var context = new MeuDbContext(options))
            {
                // Como 'CodigoDisciplina' (int) é a PK e é gerada,
                // devemos consultar pela chave de negócio 'Codigo' (string) para encontrar a disciplina salva,
                // e então verificar a PK gerada.
                var disciplinaSalva = await context.Disciplinas.FirstOrDefaultAsync(d => d.Codigo == novaDisciplina.Codigo);

                // Garante que a disciplina foi encontrada
                Assert.NotNull(disciplinaSalva);
                // Verifica se a chave primária 'CodigoDisciplina' foi gerada (não é o valor padrão 0)
                Assert.NotEqual(0, disciplinaSalva.CodigoDisciplina);
                // Verifica outras propriedades para garantir que os dados estão corretos
                Assert.Equal("Inteligência Artificial", disciplinaSalva.NomeDisciplina);
                Assert.Equal(novaDisciplina.CargaHoraria, disciplinaSalva.CargaHoraria);
                Assert.Equal(novaDisciplina.Ementa, disciplinaSalva.Ementa);
            }
        }

        /// <summary>
        /// Testa o método customizado ObterPorCodigo para uma disciplina que existe.
        /// Este método customizado deve buscar pela chave de negócio 'Codigo' (string).
        /// </summary>
        [Fact]
        public async Task ObterPorCodigo_ComCodigoExistente_DeveRetornarDisciplinaCorreta()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var codigoBusca = "EST0110";
            using (var context = new MeuDbContext(options))
            {
                // Adiciona uma disciplina para ser encontrada
                context.Disciplinas.Add(new Disciplina { Codigo = codigoBusca, NomeDisciplina = "Estatística e Probabilidade", CargaHoraria = 60 });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new DisciplinaRepository(context);
                // Chama o método customizado ObterPorCodigo (que busca pela string 'Codigo')
                var resultado = await repository.ObterPorCodigo(codigoBusca);

                // Assert
                // Verifica se a disciplina foi encontrada e se as propriedades estão corretas
                Assert.NotNull(resultado);
                Assert.Equal(codigoBusca, resultado.Codigo); // Compara o código de negócio
                Assert.Equal("Estatística e Probabilidade", resultado.NomeDisciplina);
            }
        }

        /// <summary>
        /// Testa o método customizado ObterPorCodigo para uma disciplina que não existe.
        /// Deve retornar null, indicando que não foi encontrada.
        /// </summary>
        [Fact]
        public async Task ObterPorCodigo_ComCodigoInexistente_DeveRetornarNull()
        {
            // Arrange
            var options = CreateNewContextOptions();

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new DisciplinaRepository(context);
                // Tenta buscar um código de negócio que certamente não existe
                var resultado = await repository.ObterPorCodigo("XYZ999");

                // Assert
                // Espera que o resultado seja null
                Assert.Null(resultado);
            }
        }

        /// <summary>
        /// Testa o método herdado ObterPorId.
        /// Este método deve usar a chave primária INT (CodigoDisciplina).
        /// </summary>
        [Fact]
        public async Task ObterPorId_ComChaveIntExistente_DeveRetornarDisciplina()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var novaDisciplina = new Disciplina { Codigo = "FIS0195", NomeDisciplina = "Física 2", CargaHoraria = 90 };
            int disciplinaIdGerado; // Variável para armazenar o ID int gerado pelo banco

            using (var context = new MeuDbContext(options))
            {
                // Adiciona a disciplina e salva para que o CodigoDisciplina (PK int) seja gerado
                context.Disciplinas.Add(novaDisciplina);
                await context.SaveChangesAsync();
                disciplinaIdGerado = novaDisciplina.CodigoDisciplina; // Captura o ID int gerado
            }

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new DisciplinaRepository(context);
                // Chama ObterPorId passando a chave primária INT gerada
                var resultado = await repository.ObterPorId(disciplinaIdGerado);

                // Assert
                // Verifica se a disciplina foi encontrada e se os IDs batem
                Assert.NotNull(resultado);
                Assert.Equal(disciplinaIdGerado, resultado.CodigoDisciplina); // Compara a PK
                Assert.Equal(novaDisciplina.Codigo, resultado.Codigo); // Confere também o código de negócio
            }
        }

        /// <summary>
        /// Testa se a remoção de uma disciplina existente funciona e retorna true.
        /// A remoção deve ser feita usando a chave primária INT (CodigoDisciplina).
        /// </summary>
        [Fact]
        public async Task Remover_DisciplinaExistente_DeveRetornarTrueERemoverDoBanco()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var disciplinaParaRemover = new Disciplina { Codigo = "MAT0031", NomeDisciplina = "Cálculo 2", CargaHoraria = 60 };
            int disciplinaIdParaRemover; // Variável para armazenar o ID int da disciplina a ser removida

            using (var context = new MeuDbContext(options))
            {
                // Adiciona a disciplina para que ela exista antes de tentar remover
                context.Disciplinas.Add(disciplinaParaRemover);
                await context.SaveChangesAsync();
                disciplinaIdParaRemover = disciplinaParaRemover.CodigoDisciplina; // Obtém o ID int gerado (PK)
            }

            bool resultadoDaAcao;

            // Act
            using (var context = new MeuDbContext(options))
            {
                var repository = new DisciplinaRepository(context);
                // Chama o método Remover passando a chave primária INT
                resultadoDaAcao = await repository.Remover(disciplinaIdParaRemover);
            }

            // Assert
            // Verifica se a ação de remoção foi bem-sucedida
            Assert.True(resultadoDaAcao);

            // Agora, verifica se a disciplina foi realmente removida do banco
            using (var context = new MeuDbContext(options))
            {
                // Tenta encontrar a disciplina pelo seu CodigoDisciplina (PK int)
                // ou pelo seu Codigo (código de negócio string) para confirmar a remoção.
                // Usamos FirstOrDefaultAsync para a busca flexível.
                var resultadoBusca = await context.Disciplinas.FirstOrDefaultAsync(d => d.Codigo == disciplinaParaRemover.Codigo);

                // Espera que a disciplina não seja encontrada (seja null)
                Assert.Null(resultadoBusca);
            }
        }
    }
}