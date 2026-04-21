namespace Models.Entites
{
    public class MenuItemExtra
    {
        public int MenuItemId { get; set; }
        public int ExtraId { get; set; }
        public decimal Price { get; set; }
        public MenuItem MenuItem { get; set; }
        public Extra Extra { get; set; }
    }
}