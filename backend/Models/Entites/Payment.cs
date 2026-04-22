using Models.Enums;

namespace Models.Entites
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? CashierId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public Order Order { get; set; }
        public User? Cashier { get; set; }
    }
}
