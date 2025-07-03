using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection; // Necessário para GetRequiredService
using SPAA.Data.Context; // Ajuste para o namespace do seu MeuDbContext
using SPAA.App.Tests.IntegrationTests.Factories; // Ajuste para o namespace da sua CustomWebApplicationFactory
using System;
using SPAA.App;

namespace SPAA.App.Tests.IntegrationTests.Controllers
{
    // O AssemblyFixture garante que a fábrica seja inicializada UMA VEZ para todos os testes no assembly
    // Isso é ideal para performance, pois evita recriar o servidor em memória para cada teste.
    // Substitua 'SPAA.App.Program' pela sua classe Program (se .NET 6+) ou Startup (se .NET 5 ou anterior)
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory<Program>> // REMOVIDO 'SPAA.App.' AQUI
    {
        // Esta classe não tem código, mas define o CollectionFixture
        // e aplica a todos os testes que usarem [Collection(nameof(IntegrationTestCollection))]
    }

    // Classe base para seus testes de integração
    // Cada teste que herdar desta classe terá acesso ao HttpClient
    // e ao serviço da CustomWebApplicationFactory.
    [Collection(nameof(IntegrationTestCollection))]
    public abstract class IntegrationTestFixture : IDisposable
    {
        protected readonly CustomWebApplicationFactory<Program> _factory; // REMOVIDO 'SPAA.App.' AQUI
        protected readonly HttpClient _client;
        protected readonly MeuDbContext _dbContext; // Adicione o DbContext para assertivas diretas no DB, se necessário
        private readonly IServiceScope _scope; // Para gerenciar o escopo do DbContext
        private CustomWebApplicationFactory<Microsoft.VisualStudio.TestPlatform.TestHost.Program> factory;

        public IntegrationTestFixture(CustomWebApplicationFactory<Program> factory) // REMOVIDO 'SPAA.App.' AQUI
        {
            _factory = factory;
            _client = _factory.CreateClient(); // Cria um cliente HTTP para fazer requisições à aplicação


            // *** MUDANÇA AQUI: Configurar HttpClient para não seguir redirecionamentos ***
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Desabilita o redirecionamento automático
            });

            _scope = _factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<MeuDbContext>();
        }

        protected IntegrationTestFixture(CustomWebApplicationFactory<Microsoft.VisualStudio.TestPlatform.TestHost.Program> factory)
        {
            this.factory = factory;
        }

        // Método Dispose para limpar recursos
        public void Dispose()
        {
            _client.Dispose();
            _scope.Dispose(); // Garante que o escopo e, consequentemente, o DbContext sejam descartados.
                              // A fábrica (factory) é descartada pelo XUnit via IDisposable da CustomWebApplicationFactory,
                              // ou pelo CollectionFixture.
        }
    }
}