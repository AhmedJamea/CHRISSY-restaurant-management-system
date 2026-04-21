using Models.Enums;

namespace Models.Entites
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int ProductId { get; set; }
        public ItemSize Size { get; set; } = ItemSize.regular; //assigning the default value directly
        public decimal Price { get; set; }
        public Product Product { get; set; }
        public ICollection<MenuItemExtra> MenuItemExtras { get; set; } = new List<MenuItemExtra>();
        public Branch Branch { get; set; }
    }
}
