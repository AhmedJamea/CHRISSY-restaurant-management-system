using Models.Enums;

namespace Models.Entites
{
    public class OrderTransaction
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int CashierId { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Timestamp { get; set; }

        public Order Order { get; set; }
        public User User { get; set; }
    }
}
