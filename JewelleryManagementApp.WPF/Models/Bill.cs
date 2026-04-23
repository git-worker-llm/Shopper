using System;
using System.Collections.Generic;

namespace JewelleryManagementApp.WPF.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime Date { get; set; }
        public double TotalAmount { get; set; }
        public double GSTAmount { get; set; }
        public List<BillItem> BillItems { get; set; } = new List<BillItem>();
    }
}
