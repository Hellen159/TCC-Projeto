using SPAA.App.Configurations;
using SPAA.Business.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona servi�os da aplica��o
builder.Services.AddDependencyInjection(builder.Configuration);

var app = builder.Build();

// Configura o pipeline da aplica��o
app.UseApplicationPipeline();

app.Run();

public partial class Program { }