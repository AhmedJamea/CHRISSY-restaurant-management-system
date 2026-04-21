using Models.Enums;

namespace Models.Entites
{
    public class BranchTable
    {
        public int BranchId { get; set; }
        public int TableNumber { get; set; }
        public TableStatus Status { get; set; }
        public Branch Branch { get; set; } = null!;
    }
}
