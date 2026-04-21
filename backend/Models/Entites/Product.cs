namespace Models.Entites
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }

}
