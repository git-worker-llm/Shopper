using System.Windows;
using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;

namespace JewelleryManagementApp.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
