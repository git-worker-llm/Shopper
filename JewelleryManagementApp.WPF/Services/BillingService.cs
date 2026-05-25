using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;

namespace JewelleryManagementApp.WPF.Services
{
    public class BillingService : IBillingService
    {
        public BillingService(JewelleryDbContext dbContext)
        {
            // Keep constructor for DI compatibility, but we will use fresh instances below
        }

        public async Task<List<Item>> GetAvailableItemsAsync()
        {
            using (var db = new JewelleryDbContext())
            {
                return await db.Items.ToListAsync();
            }
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            using (var db = new JewelleryDbContext())
            {
                return await db.Customers.ToListAsync();
            }
        }

        public async Task SaveBillAsync(Bill bill)
        {
            using (var db = new JewelleryDbContext())
            {
                db.Bills.Add(bill);
                await db.SaveChangesAsync();
            }
        }
    }
}
