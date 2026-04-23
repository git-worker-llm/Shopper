using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;

namespace JewelleryManagementApp.WPF.Services
{
    public class BillingService : IBillingService
    {
        private readonly JewelleryDbContext _dbContext;

        public BillingService(JewelleryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Item>> GetAvailableItemsAsync() => await _dbContext.Items.ToListAsync();
        public async Task<List<Customer>> GetCustomersAsync() => await _dbContext.Customers.ToListAsync();

        public async Task SaveBillAsync(Bill bill)
        {
            _dbContext.Bills.Add(bill);
            await _dbContext.SaveChangesAsync();
        }
    }
}
