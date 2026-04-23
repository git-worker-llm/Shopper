using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class BillingView : UserControl
    {
        public BillingView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<BillingViewModel>();
        }
    }
}
