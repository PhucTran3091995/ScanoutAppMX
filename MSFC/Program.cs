using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFC.Data;
using MSFC.Service;
using MSFC.Service.Translate;
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
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "ThreadException");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                // e.ExceptionObject là object, phải cast về Exception
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show(ex?.ToString() ?? e.ExceptionObject.ToString(), "UnhandledException");
            };


            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                var host = Host.CreateDefaultBuilder()
                    // đảm bảo content root đúng khi chạy từ nơi khác
                    .UseContentRoot(AppContext.BaseDirectory)
                    .ConfigureServices((ctx, services) =>
                    {
                        //services.AddLogging(b =>
                        //{
                        //    b.AddConsole();
                        //    // TODO: thêm b.AddFile(...) nếu dùng Serilog/NLog
                        //});

                        services.AddSingleton<IConfiguration>(configuration);
                        services.Configure<UiAutomationSettings>(configuration.GetSection("UiAutomation"));
                        services.Configure<AutomationConfig2>(configuration.GetSection("AutomationConfig2"));
                        services.Configure<TranslationConfig>(configuration.GetSection("TranslationConfig"));

                        //var conn = configuration.GetConnectionString("DefaultConnection");
                        //var serverVersion = new MySqlServerVersion(new Version(9, 1, 0)); // chỉnh đúng
                        //services.AddDbContextFactory<MMesDbContext>(o => o.UseMySql(conn, serverVersion));

                        var conn = configuration.GetConnectionString("LocalConnection");

                        services.AddDbContextFactory<MMesDbContext>(options =>
                        {
                            options.UseMySql(conn, ServerVersion.AutoDetect(conn),
                                mySqlOptions =>
                                {
                                    // tuỳ chọn hữu ích:
                                    mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                                });

                            // Bật log để thấy lỗi chi tiết
                            options.EnableSensitiveDataLogging();   // chỉ bật ở dev!
                            options.EnableDetailedErrors();
                        });


                        services.AddSingleton<ILoggingService, LoggingService>();

                        // Trì hoãn UI automation: đừng làm việc nặng trong ctor/singleton
                        services.AddSingleton<IAutoScanOutUI, AutoScanOutUI>();
                        services.AddSingleton<IAutomationService2, AutomationService2>();
                        services.AddSingleton<ITranslatorService, TranslatorService>();

                        services.AddTransient<Form1>();
                    })
                    .Build();

                ApplicationConfiguration.Initialize();
                var form = host.Services.GetRequiredService<Form1>();
                Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Startup error"); // **quan trọng**
            }
        }

    }
}