namespace Models.Entites
{
    public class Extra
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<MenuItemExtra> MenuItemExtras { get; set; } = new List<MenuItemExtra>();
    }
}
