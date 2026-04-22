namespace Models.Entites
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public decimal ItemPrice { get; set; } // The historical snapshot
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public Order Order { get; set; }
        public MenuItem MenuItem { get; set; }
        public ICollection<OrderItemExtra> OrderItemExtras { get; set; } = new List<OrderItemExtra>();
    }
}