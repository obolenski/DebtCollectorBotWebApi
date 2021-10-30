using DebtCollectorBotWebApi.Data;
using DebtCollectorBotWebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DebtCollectorBotWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddSingleton<ITelegramBotService, TelegramBotService>();
            services.AddSingleton<IDispatcher, Dispatcher>();
            services.AddSingleton<IAccountingService, AccountingService>();
            services.AddSingleton<IMongoService, MongoService>();

            services.AddHostedService<LifecycleService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
