using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<InventoryViewModel>();
        }
    }
}
