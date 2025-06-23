using Microsoft.AspNetCore.Localization;
using SPAA.App.Middlewares;
using System.Globalization;

namespace SPAA.App.Configurations
{
    public static class PipelineConfig
    {
        public static WebApplication UseApplicationPipeline(this WebApplication app)
        {
            // Middleware de tratamento de erros personalizado
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Configure o pipeline HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            var defaultCulture = new CultureInfo("pt-BR");
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(defaultCulture),
                SupportedCultures = new List<CultureInfo> { defaultCulture },
                SupportedUICultures = new List<CultureInfo> { defaultCulture }
            };
            app.UseRequestLocalization(localizationOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return app;
        }
    }
}
