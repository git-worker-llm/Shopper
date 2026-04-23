using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
using System.ComponentModel;
using System.Windows.Data;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public ObservableCollection<Bill> Bills { get; set; }
        public ICollectionView BillsView { get; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                BillsView.Refresh();
            }
        }

        public HistoryViewModel()
        {
            Bills = new ObservableCollection<Bill>();
            BillsView = CollectionViewSource.GetDefaultView(Bills);
            BillsView.Filter = FilterBills;
            Refresh();
        }

        private bool FilterBills(object bill)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return ((Bill)bill).Customer?.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true;
        }

        public override void Refresh()
        {
            Bills.Clear();
            using (var context = new JewelleryDbContext())
            {
                foreach (var bill in context.Bills.Include(b => b.Customer).ToList())
                {
                    Bills.Add(bill);
                }
            }
            BillsView.Refresh();
        }
    }
}
