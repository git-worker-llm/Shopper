using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class CustomerView : UserControl
    {
        public CustomerView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<CustomerViewModel>();
        }
    }
}
