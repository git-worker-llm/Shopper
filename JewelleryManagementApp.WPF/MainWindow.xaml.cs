using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;
using JewelleryManagementApp.WPF.Views;

namespace JewelleryManagementApp.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Assuming we set DataContext for views that need it
            // This is a quick fix, ideally use a factory or better DI integration
            if (BillingTab.Content is UserControl billing) billing.DataContext = App.ServiceProvider.GetRequiredService<BillingViewModel>();

            // Find the Print Setup tab and set its DataContext
            var tabControl = (TabControl)Content;
            foreach (TabItem item in tabControl.Items)
            {
                if (item.Header.ToString() == "Print Setup" && item.Content is UserControl printView)
                {
                    printView.DataContext = App.ServiceProvider.GetRequiredService<PrintTemplateViewModel>();
                }
                if (item.Header.ToString() == "History" && item.Content is UserControl historyView)
                {
                    historyView.DataContext = App.ServiceProvider.GetRequiredService<HistoryViewModel>();
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only react if the event originated from the TabControl itself
            if (e.Source != sender) return;

            if (BillingTab.IsSelected && BillingTab.Content is UserControl billingView)
            {
                if (billingView.DataContext is BillingViewModel viewModel)
                {
                    viewModel.OnTabSelected();
                }
            }
        }
    }
}
