namespace Models.Entites
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<BranchTable> Tables { get; set; } = new List<BranchTable>();
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    }
}
