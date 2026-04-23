using System.Linq;
using JewelleryManagementApp.WPF.Data;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public double TotalSales { get; set; }
        public int TotalItemsInStock { get; set; }

        public DashboardViewModel()
        {
            Refresh();
        }

        public override void Refresh()
        {
            using (var context = new JewelleryDbContext())
            {
                TotalSales = context.Bills.Sum(b => b.TotalAmount);
                TotalItemsInStock = context.Items.Sum(i => i.Stock);
            }
            OnPropertyChanged(nameof(TotalSales));
            OnPropertyChanged(nameof(TotalItemsInStock));
        }
    }
}
