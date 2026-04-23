using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<SettingsViewModel>();
        }
    }
}
