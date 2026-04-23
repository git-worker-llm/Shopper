using System;
using System.Windows;
using System.Windows.Threading;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Services;
using JewelleryManagementApp.WPF.ViewModels;
using JewelleryManagementApp.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryManagementApp.WPF
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<JewelleryDbContext>();
            services.AddSingleton<IBillingService, BillingService>();
            services.AddSingleton<IExceptionService, ExceptionService>();
            services.AddSingleton<IInvoiceService, InvoiceService>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<BillingViewModel>();
            services.AddSingleton<InventoryViewModel>();
            services.AddSingleton<CustomerViewModel>();
            services.AddSingleton<HistoryViewModel>();
            services.AddSingleton<GSTReportViewModel>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<SettingsViewModel>();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exceptionService = ServiceProvider.GetRequiredService<IExceptionService>();
            exceptionService.HandleException(e.Exception);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using (var db = new JewelleryDbContext())
            {
                db.Database.EnsureCreated();
            }
            var loginView = new LoginView();
            loginView.Show();
        }
    }
}
