using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class GSTReportView : UserControl
    {
        public GSTReportView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<GSTReportViewModel>();
        }
    }
}
