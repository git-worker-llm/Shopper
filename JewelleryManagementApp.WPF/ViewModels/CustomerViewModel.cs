using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Commands;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private readonly JewelleryDbContext _dbContext;

        public ObservableCollection<Customer> Customers { get; set; }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); Validate(); } }

        private string _phone = string.Empty;
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

        public ICommand AddCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        public CustomerViewModel()
        {
            _dbContext = new JewelleryDbContext();
            Customers = new ObservableCollection<Customer>(_dbContext.Customers.ToList());
            AddCustomerCommand = new RelayCommand(_ => AddCustomer(), _ => string.IsNullOrEmpty(this[nameof(Name)]));
            DeleteCustomerCommand = new RelayCommand(p => DeleteCustomer((Customer)p!));
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) AddError(nameof(Name), "Name is required");
            else ClearError(nameof(Name));
        }

        public override void Refresh()
        {
            Customers.Clear();
            foreach (var customer in _dbContext.Customers.ToList())
            {
                Customers.Add(customer);
            }
        }

        private void AddCustomer()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;
            var customer = new Customer { Name = Name, Phone = Phone };
            _dbContext.Customers.Add(customer);
            _dbContext.SaveChanges();
            Customers.Add(customer);
            Name = string.Empty;
            Phone = string.Empty;
        }

        private void DeleteCustomer(Customer customer)
        {
            _dbContext.Customers.Remove(customer);
            _dbContext.SaveChanges();
            Customers.Remove(customer);
        }
    }
}
