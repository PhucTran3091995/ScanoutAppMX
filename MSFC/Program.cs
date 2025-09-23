using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFC.Data;
using MSFC.UI_Automation;
using ScanOutTool.Services;

namespace MSFC
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Load config (appsettings.json)
            var configuration = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                // Logging (console/file etc. as you like)
                services.AddLogging(b => b.AddConsole());

                services.AddSingleton<IConfiguration>(configuration);
                services.Configure<UiAutomationSettings>(configuration.GetSection("UiAutomation"));

                var conn = ctx.Configuration.GetConnectionString("DefaultConnection");
                // Option B: Register factory
                services.AddDbContextFactory<MMesDbContext>(o =>
                    o.UseMySql(conn, ServerVersion.AutoDetect(conn)));

                // UI automation service
                services.AddSingleton<IAutoScanOutUI, AutoScanOutUI>(); // singleton: one timer/attachment

                // logging service
                services.AddSingleton<ILoggingService, LoggingService>();

                // Register form
                services.AddTransient<Form1>();
            })
            .Build();

            ApplicationConfiguration.Initialize();

            // IMPORTANT: Get Form1 from DI
            var form = host.Services.GetRequiredService<Form1>();
            Application.Run(form);
        }
    }
}