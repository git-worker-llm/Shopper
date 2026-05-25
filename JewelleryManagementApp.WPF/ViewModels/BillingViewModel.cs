using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Commands;
using JewelleryManagementApp.WPF.Helpers;
using JewelleryManagementApp.WPF.Services;
using JewelleryManagementApp.WPF.Data;
using Microsoft.EntityFrameworkCore;
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

        private string _quickAddCustomerName = string.Empty;
        public string QuickAddCustomerName { get => _quickAddCustomerName; set { _quickAddCustomerName = value; OnPropertyChanged(); } }

        private string _quickAddCustomerPhone = string.Empty;
        public string QuickAddCustomerPhone { get => _quickAddCustomerPhone; set { _quickAddCustomerPhone = value; OnPropertyChanged(); } }

        private bool _isQuickAddOpen;
        public bool IsQuickAddOpen { get => _isQuickAddOpen; set { _isQuickAddOpen = value; OnPropertyChanged(); } }

        public ICommand OpenQuickAddCommand { get; }
        public ICommand CloseQuickAddCommand { get; }
        public ICommand SaveQuickAddCustomerCommand { get; }

        public BillingViewModel(IBillingService billingService, IInvoiceService invoiceService)
        {
            _billingService = billingService;
            _invoiceService = invoiceService;
            //InitializeData();
            AddCommand = new RelayCommand(_ => AddItemToBill(), _ => SelectedItem != null);
            SaveBillCommand = new RelayCommand(async _ => await SaveBillAsync(), _ => CurrentBillItems.Any() && SelectedCustomer != null);
            OpenQuickAddCommand = new RelayCommand(_ => { QuickAddCustomerName = ""; QuickAddCustomerPhone = ""; IsQuickAddOpen = true; });
            CloseQuickAddCommand = new RelayCommand(_ => IsQuickAddOpen = false);
            SaveQuickAddCustomerCommand = new RelayCommand(async _ => await SaveQuickAddCustomerAsync(), _ => !string.IsNullOrWhiteSpace(QuickAddCustomerName));
        }

        private async Task SaveQuickAddCustomerAsync()
        {
            if (string.IsNullOrWhiteSpace(QuickAddCustomerName))
            {
                System.Windows.MessageBox.Show("Please enter a customer name.", "Validation Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var newCustomer = new Customer
            {
                Name = QuickAddCustomerName.Trim(),
                Phone = QuickAddCustomerPhone?.Trim() ?? string.Empty
            };

            try
            {
                using (var db = new JewelleryDbContext())
                {
                    db.Customers.Add(newCustomer);
                    await db.SaveChangesAsync();
                }

                // Reload active collections
                await LoadDataAsync();

                // Auto-select the newly created customer
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == newCustomer.Id);

                IsQuickAddOpen = false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to add customer: {ex.Message}", "Database Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
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

            // Load GST Rate from the Settings table
            double gstRatePercentage = 3.0; // Default Indian GST rate for jewellery is 3%
            using (var db = new JewelleryDbContext())
            {
                var settings = db.Settings.FirstOrDefault();
                if (settings != null)
                {
                    gstRatePercentage = settings.GSTRate;
                }
            }
            double gstRateFactor = gstRatePercentage / 100.0;

            var bill = new Bill
            {
                CustomerId = SelectedCustomer.Id,
                Date = DateTime.Now,
                TotalAmount = TotalAmount,
                GSTAmount = TotalAmount * gstRateFactor
            };

            // Process items, adjust inventory stocks, and detach entities to avoid duplicate insertion crashes
            foreach (var item in CurrentBillItems)
            {
                var dbBillItem = new BillItem
                {
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                bill.BillItems.Add(dbBillItem);

                // Reduce inventory stock in DB
                using (var db = new JewelleryDbContext())
                {
                    var dbItem = db.Items.FirstOrDefault(i => i.Id == item.ItemId);
                    if (dbItem != null)
                    {
                        dbItem.Stock = Math.Max(0, dbItem.Stock - item.Quantity);
                        db.SaveChanges();
                    }
                }
            }

            // Save the bill
            await _billingService.SaveBillAsync(bill);

            // Fetch the fully populated bill for printing so it prints customer name and item names beautifully
            using (var db = new JewelleryDbContext())
            {
                var fullBill = db.Bills
                    .Include(b => b.Customer)
                    .Include(b => b.BillItems)
                        .ThenInclude(bi => bi.Item)
                    .FirstOrDefault(b => b.Id == bill.Id);

                if (fullBill != null)
                {
                    await _invoiceService.PrintInvoiceAsync(fullBill);
                }
                else
                {
                    // Fallback in-memory assembly
                    bill.Customer = SelectedCustomer;
                    foreach (var bi in bill.BillItems)
                    {
                        bi.Item = CurrentBillItems.FirstOrDefault(c => c.ItemId == bi.ItemId)?.Item;
                    }
                    await _invoiceService.PrintInvoiceAsync(bill);
                }
            }

            // Clear current bill
            CurrentBillItems.Clear();
            CalculateTotals();

            // Refresh available items and stock in UI
            await LoadDataAsync();

            System.Windows.MessageBox.Show("Invoice processed and saved successfully! Sent to printer.", "Billing Successful", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void CalculateTotals()
        {
            TotalAmount = CurrentBillItems.Sum(x => x.Price);
        }
    }
}
