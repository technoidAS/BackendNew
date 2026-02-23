using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Config
{
    public class BackendDbContext : DbContext
    {
        public BackendDbContext(DbContextOptions<BackendDbContext> option) : base(option)
        {
            
        }

        public DbSet<UserModal> Users { get; set; }
        public DbSet<AddressModal> Addresses { get; set; }
        public DbSet<ProductModal> Products { get; set; }
        public DbSet<CartModal> Carts { get; set; }
        public DbSet<CartItemModal> CartItems { get; set; }
        public DbSet<OrderModal> Orders { get; set; }
        public DbSet<OrderItemModal> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<UserModal>()
                .HasOne(u => u.Address)
                .WithOne(a => a.User)
                .HasForeignKey<AddressModal>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<UserModal>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<CartModal>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<UserModal>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<CartModal>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

           
            modelBuilder.Entity<CartItemModal>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<OrderModal>()
                .HasOne(o => o.Address)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

           
            modelBuilder.Entity<OrderModal>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItemModal>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AddressModal>()
                .HasIndex(a => a.UserId)
                .IsUnique();


            modelBuilder.Entity<CartModal>()
                .HasIndex(c => c.UserId)
                .IsUnique();


            modelBuilder.Entity<ProductModal>()
                .Property(p => p.Price)
                .HasPrecision(10, 2);


            modelBuilder.Entity<OrderModal>()
                .Property(o => o.TotalAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItemModal>()
                .Property(oi => oi.PriceAtOrder)
                .HasPrecision(10, 2);
        }
    }
}
