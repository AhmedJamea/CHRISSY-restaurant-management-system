using Models.Enums;

namespace Models.Entites
{
    public class Order
    {
        public int Id { get; set; }
        public OrderType Type { get; set; }
        public DeliveryDestination? Destination { get; set; }
        public int BranchId { get; set; }
        public int? TableNumber { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Open;
        public Branch Branch { get; set; }
        public BranchTable? BranchTable { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
