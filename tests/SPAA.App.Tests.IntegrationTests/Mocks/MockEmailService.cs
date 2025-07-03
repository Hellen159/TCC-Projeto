// SPAA.App.Tests/Mocks/MockEmailService.cs
using System.Threading.Tasks;
using SPAA.Business.Interfaces.Services; // Assumindo que IEmailService está neste namespace

namespace SPAA.App.Tests.IntegrationTests.Mocks
{
    public class MockEmailService : IEmailService
    {
        // Propriedades para verificar se o EnviarEmailAsync foi chamado, se necessário para testes mais avançados
        public string LastSentEmail { get; private set; }
        public string LastSentSubject { get; private set; }
        public string LastSentMessage { get; private set; }

        // Mude o nome do método de SendEmailAsync para EnviarEmailAsync
        public Task EnviarEmailAsync(string email, string assunto, string mensagem)
        {
            // Não faz nada, apenas simula o envio do e-mail.
            // Para depuração, você pode adicionar um Console.WriteLine ou log aqui.
            LastSentEmail = email;
            LastSentSubject = assunto;
            LastSentMessage = mensagem;
            return Task.CompletedTask;
        }
    }
}