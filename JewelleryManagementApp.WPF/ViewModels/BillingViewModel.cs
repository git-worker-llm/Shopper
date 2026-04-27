using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Commands;
using JewelleryManagementApp.WPF.Helpers;
using JewelleryManagementApp.WPF.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class BillingViewModel : ViewModelBase
    {
        private readonly IBillingService _billingService;
        private readonly IInvoiceService _invoiceService;

        public ObservableCollection<Item> AvailableItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<BillItem> CurrentBillItems { get; set; } = new ObservableCollection<BillItem>();
        public ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>();

        private Item? _selectedItem;
        public Item? SelectedItem { get => _selectedItem; set {
            System.Diagnostics.Debug.WriteLine($"Setting SelectedItem: {value?.Name}");
            _selectedItem = value; OnPropertyChanged(); } }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer { get => _selectedCustomer;
        set {
            System.Diagnostics.Debug.WriteLine($"Setting SelectedCustomer: {value?.Name}");
            _selectedCustomer = value;
            OnPropertyChanged();
            } }

        private int _quantity = 1;
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); } }

        private double _totalAmount;
        public double TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveBillCommand { get; }

        public BillingViewModel(IBillingService billingService, IInvoiceService invoiceService)
        {
            _billingService = billingService;
            _invoiceService = invoiceService;
            //InitializeData();
            AddCommand = new RelayCommand(_ => AddItemToBill(), _ => SelectedItem != null);
            SaveBillCommand = new RelayCommand(async _ => await SaveBillAsync(), _ => CurrentBillItems.Any() && SelectedCustomer != null);
        }

        private async void InitializeData() => await LoadDataAsync();

        private async Task LoadDataAsync()
        {
            var items = await _billingService.GetAvailableItemsAsync();
            var customers = await _billingService.GetCustomersAsync();

            var currentCustomerId = SelectedCustomer?.Id;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                AvailableItems.Clear();
                foreach (var item in items) AvailableItems.Add(item);

                Customers.Clear();
                foreach (var customer in customers) Customers.Add(customer);

                if (currentCustomerId.HasValue)
                {
                    SelectedCustomer = Customers.FirstOrDefault(c => c.Id == currentCustomerId.Value);
                }
            });
        }

        public override async void Refresh() => await LoadDataAsync();

        // Added to handle automatic refresh when Billing tab is selected
        public void OnTabSelected()
        {
            Refresh();
        }

        private void AddItemToBill()
        {
            if (SelectedItem == null) return;
            var billItem = new BillItem
            {
                ItemId = SelectedItem.Id,
                Item = SelectedItem,
                Quantity = Quantity,
                Price = SelectedItem.Price * Quantity
            };
            CurrentBillItems.Add(billItem);
            CalculateTotals();
        }

        private async Task SaveBillAsync()
        {
            if (SelectedCustomer == null) return;

            var gstRate = ConfigurationHelper.Configuration.GetValue<double>("Settings:GSTRate");

            var bill = new Bill
            {
                CustomerId = SelectedCustomer.Id,
                Date = DateTime.Now,
                TotalAmount = TotalAmount,
                GSTAmount = TotalAmount * gstRate
            };

            foreach (var item in CurrentBillItems)
            {
                bill.BillItems.Add(item);
            }

            await _billingService.SaveBillAsync(bill);
            await _invoiceService.PrintInvoiceAsync(bill);

            // Clear current bill after saving
            CurrentBillItems.Clear();
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            TotalAmount = CurrentBillItems.Sum(x => x.Price);
        }
    }
}
