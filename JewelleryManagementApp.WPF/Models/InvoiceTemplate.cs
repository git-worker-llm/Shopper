namespace JewelleryManagementApp.WPF.Models
{
    public class InvoiceTemplate
    {
        public int Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TemplateXaml { get; set; } = string.Empty;
        public string AccentColor { get; set; } = "Gold";
        public double HeaderFontSize { get; set; } = 22.0;
        public string SelectedFontFamily { get; set; } = "Segoe UI";
        public string FooterText { get; set; } = "Thank you for shopping with us! Terms: Gold sold can be exchanged at current rate less melting loss.";
    }
}
