using Microsoft.AspNetCore.Builder;

namespace imcd_ticket_sync
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //add services if needed
            services.AddControllersWithViews();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Configure other error handling middleware for production
                // app.UseExceptionHandler("/Home/Error");
                // app.UseHsts();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
