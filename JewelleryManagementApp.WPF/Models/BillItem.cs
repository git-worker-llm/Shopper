namespace JewelleryManagementApp.WPF.Models
{
    public class BillItem
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public Bill? Bill { get; set; }
        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
