namespace JewelleryManagementApp.WPF.Models
{
    public class InvoiceTemplate
    {
        public int Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TemplateXaml { get; set; } = string.Empty;
    }
}
