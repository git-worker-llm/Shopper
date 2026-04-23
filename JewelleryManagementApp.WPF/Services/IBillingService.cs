using System.Collections.Generic;
using System.Threading.Tasks;
using JewelleryManagementApp.WPF.Models;

namespace JewelleryManagementApp.WPF.Services
{
    public interface IBillingService
    {
        Task<List<Item>> GetAvailableItemsAsync();
        Task<List<Customer>> GetCustomersAsync();
        Task SaveBillAsync(Bill bill);
    }
}
