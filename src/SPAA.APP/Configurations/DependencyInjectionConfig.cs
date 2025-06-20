using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Business.Services;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System.Globalization;

namespace SPAA.App.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            // Obter a string de conexão do appsettings.json
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configurar o Entity Framework Core para usar MySQL com Pomelo
            services.AddDbContext<MeuDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddEntityFrameworkStores<MeuDbContext>()
            .AddDefaultTokenProviders();

            // AutoMapper
            services.AddAutoMapper(typeof(Program).Assembly);

            // Injeção de dependências
            services.AddScoped<MeuDbContext>();
            services.AddScoped<IAlunoRepository, AlunoRepository>();
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();
            services.AddScoped<IAlunoDisciplinaRepository, AlunoDisciplinaRepository>();
            services.AddScoped<ITurmaRepository, TurmaRepository>();
            services.AddScoped<ICurriculoRepository, CurriculoRepository>();
            services.AddScoped<IPreRequisitoRepository, PreRequisitoRepository>();
            services.AddScoped<IAreaInteresseAlunoRepository, AreaInteresseAlunoRepository>();
            services.AddScoped<ICursoRepository, CursoRepository>();
            services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
            services.AddScoped<ITurmaSalvaRepository, TurmaSalvaRepository>();
            services.AddScoped<ITarefaAlunoRepository, TarefaAlunoRepository>();

            services.AddScoped<IAulaHorarioService, AulaHorarioService>();
            services.AddScoped<IAlunoDisciplinaService, AlunoDisciplinaService>();
            services.AddScoped<IDisciplinaService, DisciplinaService>();
            services.AddScoped<IPreRequisitoService, PreRequisitoService>();
            services.AddScoped<IAlunoService, AlunoService>();
            services.AddScoped<ITurmaService, TurmaService>();

            // Configurações de e-mail
            var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
            services.AddSingleton(emailSettings);
            services.AddTransient<IEmailService, EmailService>();

            // MVC com views
            services.AddControllersWithViews(o =>
            {
                o.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) => "O valor preenchido é inválido para este campo.");
                o.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(x => "Este campo precisa ser preenchido.");
                o.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "Este campo precisa ser preenchido.");
                o.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() => "É necessário que o body na requisição não esteja vazio.");
                o.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor((x) => "O valor preenchido é inválido para este campo.");
                o.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor preenchido é inválido para este campo.");
                o.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => "O valor preenchido é inválido para este campo.");
                o.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor((x) => "O valor preenchido é inválido para este campo.");
                o.ModelBindingMessageProvider.SetValueIsInvalidAccessor((x) => "Este campo precisa ser preenchido.");
                o.ModelBindingMessageProvider.SetValueMustBeANumberAccessor((x) => "O campo deve ser numérico.");
                o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor((x) => "Este campo precisa ser preenchido.");
            });
            return services;
        }
    }
}
