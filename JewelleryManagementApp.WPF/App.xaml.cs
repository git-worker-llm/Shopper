using System;
using System.Windows;
using System.Windows.Threading;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
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
            services.AddSingleton<PrintTemplateViewModel>();
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

            string dbFile = "jewellery.db";
            if (System.IO.File.Exists(dbFile))
            {
                try
                {
                    // Safe verification: check if GSTRate, IsLightTheme, and AccentColor exist by querying them
                    using (var db = new JewelleryDbContext())
                    {
                        var testGst = db.Settings.Select(s => s.GSTRate).FirstOrDefault();
                        var testTheme = db.Settings.Select(s => s.IsLightTheme).FirstOrDefault();
                        var testAccent = db.InvoiceTemplates.Select(t => t.AccentColor).FirstOrDefault();
                        var countFilings = db.GstFilings.Count();
                    }
                }
                catch
                {
                    // Outdated schema, delete DB file so EnsureCreated() recreates it with new schema
                    try
                    {
                        System.IO.File.Delete(dbFile);
                    }
                    catch { }
                }
            }

            using (var db = new JewelleryDbContext())
            {
                db.Database.EnsureCreated();

                // 1. Seed default Settings if empty
                if (!db.Settings.Any())
                {
                    db.Settings.Add(new Settings
                    {
                        ShopName = "AuraJewels Luxury Salon",
                        GSTNumber = "27AAAAA1111A1Z1",
                        Address = "Gold Souk, MG Road, Mumbai, India",
                        LogoPath = "",
                        GSTRate = 3.0 // 3.0% GST standard for jewellery in India
                    });
                }

                // 2. Seed beautiful default print Invoice Template if empty
                if (!db.InvoiceTemplates.Any())
                {
                    db.InvoiceTemplates.Add(new InvoiceTemplate
                    {
                        ShopName = "AuraJewels Luxury Salon",
                        GSTNumber = "27AAAAA1111A1Z1",
                        Address = "Gold Souk, MG Road, Mumbai, India",
                        TemplateXaml = @"<FlowDocument xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" PagePadding=""40"" FontFamily=""Segoe UI"" Background=""#121212"">
    <Paragraph FontSize=""28"" FontWeight=""Bold"" Foreground=""#D4AF37"" TextAlignment=""Center"" Margin=""0,0,0,5"">{{ShopName}}</Paragraph>
    <Paragraph FontSize=""12"" Foreground=""#A0A0A0"" TextAlignment=""Center"" Margin=""0,0,0,20"">
        Address: {{Address}} | GSTIN: {{GSTNumber}}
    </Paragraph>
    <Paragraph FontSize=""14"" Foreground=""#E0E0E0"" FontWeight=""SemiBold"" Margin=""0,10,0,5"">
        Date: {{Date}}
    </Paragraph>
</FlowDocument>"
                    });
                }

                // 3. Seed premium high-end items if inventory is empty
                if (!db.Items.Any())
                {
                    db.Items.AddRange(
                        new Item { Name = "18K Gold Diamond Solitaire Ring", Category = "Gold", Weight = 3.8, Price = 85000, Stock = 12 },
                        new Item { Name = "22K Gold Antique Bridal Necklace", Category = "Gold", Weight = 45.5, Price = 320000, Stock = 4 },
                        new Item { Name = "925 Sterling Silver Floral Anklet", Category = "Silver", Weight = 15.2, Price = 4500, Stock = 30 },
                        new Item { Name = "Platinum Infinity Couple Band", Category = "Platinum", Weight = 6.2, Price = 55000, Stock = 8 },
                        new Item { Name = "18K Gold Emerald Drop Earrings", Category = "Gold", Weight = 12.4, Price = 95000, Stock = 6 }
                    );
                }

                // 4. Seed initial clients if list is empty
                if (!db.Customers.Any())
                {
                    db.Customers.AddRange(
                        new Customer { Name = "Rajesh Mehta", Phone = "+91 98765 43210" },
                        new Customer { Name = "Priya Sharma", Phone = "+91 99887 76655" },
                        new Customer { Name = "Amit Patel", Phone = "+91 91234 56789" }
                    );
                }

                // 5. Seed some sample historical bills and filings so the dashboard charts & tables look rich instantly
                if (!db.Bills.Any())
                {
                    var customer = new Customer { Name = "Suresh Raina", Phone = "+91 95555 12345" };
                    db.Customers.Add(customer);
                    db.SaveChanges();

                    var bill1 = new Bill
                    {
                        CustomerId = customer.Id,
                        Date = DateTime.Now.AddDays(-5),
                        TotalAmount = 85000,
                        GSTAmount = 2550
                    };
                    bill1.BillItems.Add(new BillItem { ItemId = 1, Quantity = 1, Price = 85000 });
                    db.Bills.Add(bill1);

                    var bill2 = new Bill
                    {
                        CustomerId = customer.Id,
                        Date = DateTime.Now.AddDays(-2),
                        TotalAmount = 55000,
                        GSTAmount = 1650
                    };
                    bill2.BillItems.Add(new BillItem { ItemId = 4, Quantity = 1, Price = 55000 });
                    db.Bills.Add(bill2);
                }

                // Retrieve actual theme setting from DB and apply it
                var appSettings = db.Settings.FirstOrDefault() ?? new Settings();
                ApplyTheme(appSettings.IsLightTheme);

                db.SaveChanges();
            }

            var loginView = new LoginView();
            loginView.Show();
        }

        public static void ApplyTheme(bool isLight)
        {
            var app = Application.Current;
            if (app == null) return;

            if (isLight)
            {
                app.Resources["DarkBgBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F5F5F3"));
                app.Resources["CardBgBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                app.Resources["CardInnerBgBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EAEAEA"));
                app.Resources["TextLightBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1E1E1E"));
                app.Resources["TextMutedBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#666666"));
                app.Resources["BorderDarkBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D8D8D8"));
                app.Resources["BorderGoldBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E3C263"));
            }
            else
            {
                app.Resources["DarkBgBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#121212"));
                app.Resources["CardBgBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A1A1A"));
                app.Resources["CardInnerBgBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#222222"));
                app.Resources["TextLightBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F5F5F5"));
                app.Resources["TextMutedBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#AAAAAA"));
                app.Resources["BorderDarkBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333333"));
                app.Resources["BorderGoldBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#554411"));
            }
        }
    }
}
