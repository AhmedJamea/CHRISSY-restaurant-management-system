using Models.Enums;

namespace Models.Entites
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        
    }
}
