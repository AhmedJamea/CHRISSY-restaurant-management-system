using Microsoft.AspNetCore.Identity;

namespace Models.Entites
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; }

        // Navigation property for Branch
        public int? BranchId { get; set; }
        public Branch Branch { get; set; }
    }
}
