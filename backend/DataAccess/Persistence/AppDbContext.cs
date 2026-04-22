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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Extra> Extras { get; set; }
        public DbSet<MenuItemExtra> MenuItemExtras { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

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

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                // Constraints
                entity.HasIndex(c => c.Name)
                      .IsUnique();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.Property(p => p.Image)
                      .HasMaxLength(500);

                entity.Property(p => p.CategoryId)
                      .IsRequired();

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Size)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(m => m.Price)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(m => m.BranchId)
                      .IsRequired();

                entity.Property(m => m.ProductId)
                      .IsRequired();

                //composite unique constraint 
                entity.HasIndex(m => new { m.BranchId, m.ProductId, m.Size })
                      .IsUnique();

                entity.HasOne(m => m.Product)
                      .WithMany(p => p.MenuItems)
                      .HasForeignKey(m => m.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Branch)
                      .WithMany(b => b.MenuItems)
                      .HasForeignKey(m => m.BranchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Extra>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                // enforce unique names for extras
                entity.HasIndex(e => e.Name)
                      .IsUnique();
            });

            modelBuilder.Entity<MenuItemExtra>(entity =>
            {
                entity.HasKey(me => new { me.MenuItemId, me.ExtraId });

                entity.Property(me => me.Price)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                // configure the relationship to MenuItem
                entity.HasOne(me => me.MenuItem)
                      .WithMany(m => m.MenuItemExtras)
                      .HasForeignKey(me => me.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade); // If MenuItem is deleted, delete its extras mapping

                // configure the relationship to Extra
                entity.HasOne(me => me.Extra)
                      .WithMany(e => e.MenuItemExtras)
                      .HasForeignKey(me => me.ExtraId)
                      .OnDelete(DeleteBehavior.Cascade); // If an Extra is globally deleted, remove it from all menus
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Type)
                      .IsRequired()
                      .HasConversion<string>();

                entity.Property(o => o.Destination)
                      .HasConversion<string>();

                entity.Property(o => o.Status)
                      .IsRequired()
                      .HasConversion<string>();

                entity.HasOne(o => o.Branch)
                      .WithMany()
                      .HasForeignKey(o => o.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.BranchTable)
                      .WithMany()
                      .HasForeignKey(o => new { o.BranchId, o.TableNumber })
                      .OnDelete(DeleteBehavior.Restrict);

                // This creates a unique index on the table, but ONLY for orders with the status 'Open'
                entity.HasIndex(o => new { o.BranchId, o.TableNumber })
                      .IsUnique()
                      .HasFilter("[Status] = 'Open' AND [TableNumber] IS NOT NULL")
                      .HasDatabaseName("IX_Order_ActiveTableOrder");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.ItemPrice)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.SubTotal)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.Quantity)
                      .IsRequired();

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.MenuItem)
                      .WithMany()
                      .HasForeignKey(oi => oi.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
