using System;

namespace JewelleryManagementApp.WPF.Models
{
    public class GstFiling
    {
        public int Id { get; set; }
        public string Period { get; set; } = string.Empty; // e.g. "May 2026"
        public DateTime FilingDate { get; set; }
        public double TotalSales { get; set; }
        public double TaxableAmount { get; set; }
        public double GstAmount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
    }
}
