using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Diagnostics;
using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Data
{
    public class PatatZaakDB : DbContext
    {
        public DbSet<Order> Order { get; set; }
        public DbSet<Product> Product { get; set; }    
        public DbSet<Role> Role { get; set; }  
        public DbSet<User> User { get; set; }
        public DbSet<Addon> Addons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connection = "Data Source=.;Initial Catalog=PatatzaakDb;Integrated Security=True;Trust Server Certificate=True";
            optionsBuilder.UseSqlServer(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User - Role Configuration
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role Configuration 
            modelBuilder.Entity<Role>()
                .HasKey(r => r.RoleId);

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "admin" },
                new Role { RoleId = 2, RoleName = "user" },
                new Role { RoleId = 3, RoleName = "worker" }
            );

            // Order Configuration
            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .Property(o => o.Ordernumber)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Products)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderUser)
                .WithMany()
                .HasForeignKey(o => o.UserId);



            // Product Configuration
            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductName)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductCat)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductDescription)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductPoints)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.OrderId);
              


            modelBuilder.Entity<Product>()
                .Property(p => p.ProductQuantity)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductPrice)
                .HasColumnType("decimal(18,2)") // Prijs met precisie instellen
                .IsRequired();


            // User Configuration
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .Property(u => u.UserName)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .IsRequired();

            // Addon Configuration
            modelBuilder.Entity<Addon>()
                .HasKey(a => a.Identifier);

            modelBuilder.Entity<Addon>()
                .HasOne(a => a.Product)
                .WithMany(p => p.AvailableAddons)
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
