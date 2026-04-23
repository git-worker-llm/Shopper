namespace JewelleryManagementApp.WPF.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Gold/Silver/etc
        public double Weight { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}
