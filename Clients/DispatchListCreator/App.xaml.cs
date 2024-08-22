using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using TrackingPlPrototype.Entities.SBGV_PL;
using TrackingPlPrototype.Entities.SGBV_Cz;
using DispatchListCreator.ViewModels;
using DMS.Database.DbTransporty;
using TrackingPlPrototype.Entities.SGBV_HU;
using TrackingPlPrototype.Entities.SGBV_SK;
using DispatchListCreator.Contracts;
using DispatchListCreator.Logic;
namespace DispatchListCreator
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) => 
                {
                    //Scaffold - DbContext "Server=URAN\dms;Database=SGBV_PL;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer - OutputDir Entities\SGBV_PL
                    // Konfiguracja DbContext
                    services.AddDbContext<SgbvPlContext>(options =>
                        options.UseSqlServer(@"Server=URAN\dms;Database=SGBV_PL;Trusted_Connection=True;TrustServerCertificate=True;", o => o.UseCompatibilityLevel(120)));

                    services.AddDbContext<SgbvCzContext>(options =>
                        options.UseSqlServer(@"Server=URAN\dms;Database=SGBV_Cz;Trusted_Connection=True;TrustServerCertificate=True;", o => o.UseCompatibilityLevel(120)));
                    
                    services.AddDbContext<SgbvHuContext>(options =>
                        options.UseSqlServer(@"Server=URAN\dms;Database=SGBV_HU;Trusted_Connection=True;TrustServerCertificate=True;", o => o.UseCompatibilityLevel(120)));
                    
                    services.AddDbContext<SgbvContext>(options =>
                        options.UseSqlServer(@"Server=URAN\dms;Database=SGBV;Trusted_Connection=True;TrustServerCertificate=True;", o => o.UseCompatibilityLevel(120)));

                    services.AddDbContext<TransportyContext>(options =>
                        options.UseSqlServer(@"Server=URAN\dms;Database=transporty;Trusted_Connection=True;TrustServerCertificate=True;", o => o.UseCompatibilityLevel(120)));

                  
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<ISummaryContentCollector, SummaryContentCollector>();
                    services.AddTransient<SummaryContentCollectorAsync>();
                    services.AddTransient<IExportFileGenerator, ExportFileGenerator>();
                    services.AddTransient<IMainViewModel, MainViewModel>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            startupForm.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }

}
