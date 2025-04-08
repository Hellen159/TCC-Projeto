using System.Net;
using System.Text.Json;

namespace SPAA.App.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = new
            {
                Message = "Ocorreu um erro ao processar a requisição.",
                Details = ex.Message,
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Log do erro (pode ser aprimorado com um serviço de log)
            Console.WriteLine($"Erro: {ex.Message}");

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
