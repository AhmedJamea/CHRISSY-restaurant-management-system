namespace Models.Entites
{
    public class OrderItemExtra
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int ExtraId { get; set; }
        public decimal ExtraPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public OrderItem OrderItem { get; set; }
        public Extra Extra { get; set; }
    }
}