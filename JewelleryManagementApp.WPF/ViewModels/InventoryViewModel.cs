using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        private readonly JewelleryDbContext _context;
        public ObservableCollection<Item> Items { get; set; }
        public ICollectionView ItemsView { get; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ItemsView.Refresh();
            }
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) AddError(nameof(Name), "Name is required");
            else ClearError(nameof(Name));

            if (Price <= 0) AddError(nameof(Price), "Price must be > 0");
            else ClearError(nameof(Price));
        }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); Validate(); } }
        private string _category = string.Empty;
        public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }
        private double _weight;
        public double Weight { get => _weight; set { _weight = value; OnPropertyChanged(); } }
        private double _price;
        public double Price { get => _price; set { _price = value; OnPropertyChanged(); Validate(); } }
        private int _stock;
        public int Stock { get => _stock; set { _stock = value; OnPropertyChanged(); } }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public InventoryViewModel()
        {
            _context = new JewelleryDbContext();
            Items = new ObservableCollection<Item>(_context.Items.ToList());
            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = FilterItems;

            AddCommand = new RelayCommand(async _ => await AddItemAsync());
            DeleteCommand = new RelayCommand(async p => await DeleteItemAsync((Item)p!));
        }

        private bool FilterItems(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return ((Item)item).Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        public override async void Refresh()
        {
            var items = await _context.Items.ToListAsync();
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            ItemsView.Refresh();
        }

        private async Task AddItemAsync()
        {
            var item = new Item { Name = Name, Category = Category, Weight = Weight, Price = Price, Stock = Stock };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            Items.Add(item);
            Name = Category = string.Empty;
            Weight = Price = Stock = 0;
        }

        private async Task DeleteItemAsync(Item item)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            Items.Remove(item);
        }
    }
}
