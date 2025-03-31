using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// Obter a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar o Entity Framework Core para usar MySQL com Pomelo
builder.Services.AddDbContext<MeuDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configurações de validação de UserName
    options.User.RequireUniqueEmail = true;  // Exige que o email seja único
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Caracteres permitidos para UserName

    // Configurações de validação de Senha
    options.Password.RequireDigit = true;     // Exige que a senha tenha um dígito
    options.Password.RequireLowercase = true; // Exige que a senha tenha uma letra minúscula
    options.Password.RequireNonAlphanumeric = false; // Exige caracteres não alfanuméricos
    options.Password.RequireUppercase = true; // Exige que a senha tenha uma letra maiúscula
    options.Password.RequiredLength = 8;     // Tamanho mínimo da senha
    options.Password.RequiredUniqueChars = 1; // Número mínimo de caracteres únicos na senha

}).AddEntityFrameworkStores<MeuDbContext>()
  .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

//configuracoes do automapping 
builder.Services.AddAutoMapper(typeof(Program).Assembly);

//injecao de dependencias
builder.Services.AddScoped<MeuDbContext>();
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
