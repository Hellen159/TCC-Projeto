using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// Obter a string de conex�o do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar o Entity Framework Core para usar MySQL com Pomelo
builder.Services.AddDbContext<MeuDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configura��es de valida��o de UserName
    options.User.RequireUniqueEmail = true;  // Exige que o email seja �nico
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Caracteres permitidos para UserName

    // Configura��es de valida��o de Senha
    options.Password.RequireDigit = true;     // Exige que a senha tenha um d�gito
    options.Password.RequireLowercase = true; // Exige que a senha tenha uma letra min�scula
    options.Password.RequireNonAlphanumeric = false; // Exige caracteres n�o alfanum�ricos
    options.Password.RequireUppercase = true; // Exige que a senha tenha uma letra mai�scula
    options.Password.RequiredLength = 8;     // Tamanho m�nimo da senha
    options.Password.RequiredUniqueChars = 1; // N�mero m�nimo de caracteres �nicos na senha

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
