using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using TrackingPlPrototype.Entities.SGBV_Cz;
using Microsoft.Extensions.Options;
using TrackingPlPrototype.Entities.SGBV_HU;
using TrackingPlPrototype.Entities.SGBV_SK;
using MergingHuLabelsAppMvvm.Contracts;
using MergingHuLabelsAppMvvm.ViewModels;
using MergingHuLabelsAppMvvm.Logic;

namespace MergingHuLabelsAppMvvm
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
                    services.AddDbContext<SgbvHuContext>(options =>
                        //options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=SGBV_HU;Trusted_Connection=True;", o => o.UseCompatibilityLevel(120)));
                        options.UseSqlServer(@"Server=URAN\dms;Database=SGBV_HU;Trusted_Connection=True;", o => o.UseCompatibilityLevel(120)));
                    
                    //services.AddDbContext<>(options =>
                    //    options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=transporty;Trusted_Connection=True;", o => o.UseCompatibilityLevel(120)));
                    ////services.AddDbContext<HomedbContext>(options =>
                    //    options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=homedb;Trusted_Connection=True;", o => o.UseCompatibilityLevel(120)));

                    services.AddTransient<IMainViewModel, MainViewModel>();
                    services.AddTransient<PrepareFiles>();
                    services.AddTransient<GetVertical>();
                    services.AddTransient<GeneratorLabel>();
                    services.AddTransient<PrintersConfig>();
                    services.AddSingleton<MainWindow>();
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
