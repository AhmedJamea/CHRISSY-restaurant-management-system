using Microsoft.EntityFrameworkCore;
using Models.Entites;

namespace DataAccess.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BranchTable> BranchTables { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .IsRequired()
                    // Store the Enum as a readable string in the database
                    .HasConversion<string>();

                entity.HasOne(u => u.Branch)
                    .WithMany(b => b.Users)
                    .HasForeignKey(u => u.BranchId)
                    .OnDelete(DeleteBehavior.Restrict); // Protects branches with active users from deletion

                // The Check Constraint: Enforces the Admin/Cashier branch logic at the database level
                //entity.HasCheckConstraint(
                //    "CK_User_Role_Branch_Assignment",
                //    "(Role = 'Admin' AND BranchId IS NULL) OR (Role = 'Cashier' AND BranchId IS NOT NULL)"
                //);
            });

            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<BranchTable>(entity =>
            {
                entity.HasKey(bt => new { bt.BranchId, bt.TableNumber });

                entity.Property(bt => bt.Status)
                    .IsRequired()
                    .HasConversion<string>(); // Stores 'Available' or 'Occupied' in the DB instead of 0 or 1

                entity.HasOne(bt => bt.Branch)
                    .WithMany(b => b.Tables)
                    .HasForeignKey(bt => bt.BranchId)
                    .IsRequired() // Enforces that the Foreign Key cannot be null
                    .OnDelete(DeleteBehavior.Cascade); // Deleting a Branch deletes all its Tables
            });
        }
    }
}
