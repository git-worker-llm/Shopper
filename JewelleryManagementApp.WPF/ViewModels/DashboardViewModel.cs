using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public double TotalSales { get; set; }
        public double TotalGST { get; set; }
        public int TotalItemsInStock { get; set; }
        public int TotalCustomers { get; set; }

        public double GoldSalesPercentage { get; set; } = 65.0;
        public double SilverSalesPercentage { get; set; } = 25.0;
        public double PlatinumSalesPercentage { get; set; } = 10.0;

        public ObservableCollection<Bill> RecentSales { get; set; } = new ObservableCollection<Bill>();
        public ObservableCollection<Item> LowStockItems { get; set; } = new ObservableCollection<Item>();

        public DashboardViewModel()
        {
            Refresh();
        }

        public override void Refresh()
        {
            using (var context = new JewelleryDbContext())
            {
                var bills = context.Bills.ToList();
                var items = context.Items.ToList();
                var customers = context.Customers.ToList();

                // Compute KPI Statistics
                double totalSubtotal = bills.Sum(b => b.TotalAmount);
                double totalGst = bills.Sum(b => b.GSTAmount);
                
                TotalSales = totalSubtotal + totalGst;
                TotalGST = totalGst;
                TotalItemsInStock = items.Sum(i => i.Stock);
                TotalCustomers = customers.Count;

                // Load Recent Sales (top 5 latest) with Customers included
                RecentSales.Clear();
                var recent = context.Bills
                    .Include(b => b.Customer)
                    .OrderByDescending(b => b.Date)
                    .Take(5)
                    .ToList();

                foreach (var b in recent)
                {
                    RecentSales.Add(b);
                }

                // Load Low Stock items (Stock less than 5)
                LowStockItems.Clear();
                var lowStock = items.Where(i => i.Stock <= 5).OrderBy(i => i.Stock).ToList();
                foreach (var item in lowStock)
                {
                    LowStockItems.Add(item);
                }

                // Calculate Category Breakdowns dynamically if bills exist
                var billItems = context.BillItems.Include(bi => bi.Item).ToList();
                double goldTotal = billItems.Where(bi => bi.Item?.Category == "Gold").Sum(bi => bi.Price);
                double silverTotal = billItems.Where(bi => bi.Item?.Category == "Silver").Sum(bi => bi.Price);
                double platTotal = billItems.Where(bi => bi.Item?.Category == "Platinum").Sum(bi => bi.Price);
                double totalCat = goldTotal + silverTotal + platTotal;

                if (totalCat > 0)
                {
                    GoldSalesPercentage = Math.Round((goldTotal / totalCat) * 100, 1);
                    SilverSalesPercentage = Math.Round((silverTotal / totalCat) * 100, 1);
                    PlatinumSalesPercentage = Math.Round((platTotal / totalCat) * 100, 1);
                }
                else
                {
                    GoldSalesPercentage = 60.0;
                    SilverSalesPercentage = 30.0;
                    PlatinumSalesPercentage = 10.0;
                }
            }

            OnPropertyChanged(nameof(TotalSales));
            OnPropertyChanged(nameof(TotalGST));
            OnPropertyChanged(nameof(TotalItemsInStock));
            OnPropertyChanged(nameof(TotalCustomers));
            OnPropertyChanged(nameof(GoldSalesPercentage));
            OnPropertyChanged(nameof(SilverSalesPercentage));
            OnPropertyChanged(nameof(PlatinumSalesPercentage));
        }
    }
}
