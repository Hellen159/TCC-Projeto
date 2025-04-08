using SPAA.App.Configurations;
using SPAA.Business.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços da aplicação
builder.Services.AddDependencyInjection(builder.Configuration);

var app = builder.Build();

app.UseRequestLocalization();

// Configura o pipeline da aplicação
app.UseApplicationPipeline();

app.Run();
