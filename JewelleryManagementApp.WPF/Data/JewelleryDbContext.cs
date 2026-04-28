using Microsoft.EntityFrameworkCore;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Helpers;
using Microsoft.Extensions.Configuration;

namespace JewelleryManagementApp.WPF.Data
{
    public class JewelleryDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillItem> BillItems { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<InvoiceTemplate> InvoiceTemplates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationHelper.Configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
