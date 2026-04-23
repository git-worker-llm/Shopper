using System.Linq;
using JewelleryManagementApp.WPF.Data;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class GSTReportViewModel : ViewModelBase
    {
        public double TotalSales { get; set; }
        public double TotalGST { get; set; }

        public GSTReportViewModel()
        {
            Refresh();
        }

        public override void Refresh()
        {
            using (var context = new JewelleryDbContext())
            {
                var bills = context.Bills.ToList();
                TotalSales = bills.Sum(b => b.TotalAmount);
                TotalGST = bills.Sum(b => b.GSTAmount);
            }
            OnPropertyChanged(nameof(TotalSales));
            OnPropertyChanged(nameof(TotalGST));
        }
    }
}
