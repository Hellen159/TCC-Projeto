using SPAA.App.Middlewares;

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
