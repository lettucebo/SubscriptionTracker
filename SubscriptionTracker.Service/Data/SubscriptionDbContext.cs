using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Service.Models;

namespace SubscriptionTracker.Service.Data
{
    /// <summary>
    /// Represents the database context for subscriptions.
    /// </summary>
    public class SubscriptionDbContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }

        public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SubscriptionTracker;Trusted_Connection=True;");
            }
        }

        /// <summary>
        /// Configures the model and seeds initial data.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Subscription entity
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.Property(s => s.Amount)
                    .HasColumnType("decimal(18,2)");
                
                entity.Property(s => s.BillingCycle)
                    .HasMaxLength(10)
                    .IsRequired();
                
                entity.Property(s => s.DiscountRate)
                    .HasColumnType("decimal(3,2)")
                    .HasDefaultValue(0m);
            });

            // Seed 10 sample subscriptions
            modelBuilder.Entity<Subscription>().HasData(
                new Subscription { 
                    Id = 1, 
                    Name = "Netflix", 
                    Amount = 15.99m,
                    BillingCycle = "monthly",
                    DiscountRate = 0.0m,
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = DateTime.Today.AddDays(335),
                    Category = "Entertainment" 
                },
                new Subscription { 
                    Id = 2, 
                    Name = "Spotify", 
                    Amount = 9.99m,
                    BillingCycle = "monthly", 
                    DiscountRate = 0.0m,
                    StartDate = DateTime.Today.AddDays(-15),
                    Category = "Music" 
                },
                new Subscription { 
                    Id = 3, 
                    Name = "Amazon Prime", 
                    Amount = 139.0m,
                    BillingCycle = "yearly",
                    DiscountRate = 0.17m,
                    StartDate = DateTime.Today.AddDays(-60),
                    EndDate = DateTime.Today.AddDays(305),
                    Category = "Shopping" 
                },
                new Subscription { 
                    Id = 4, 
                    Name = "Adobe Creative Cloud", 
                    Amount = 52.99m,
                    BillingCycle = "yearly",
                    DiscountRate = 0.15m,
                    StartDate = DateTime.Today.AddDays(-90),
                    EndDate = DateTime.Today.AddDays(275),
                    Category = "Productivity" 
                },
                new Subscription { 
                    Id = 5, 
                    Name = "HBO Max", 
                    Amount = 14.99m,
                    BillingCycle = "monthly",
                    DiscountRate = 0.0m,
                    StartDate = DateTime.Today.AddDays(-20),
                    Category = "Entertainment" 
                },
                new Subscription { 
                    Id = 6, 
                    Name = "Disney+", 
                    Amount = 7.99m,
                    BillingCycle = "monthly",
                    DiscountRate = 0.0m,
                    StartDate = DateTime.Today.AddDays(-10),
                    Category = "Entertainment" 
                },
                new Subscription { 
                    Id = 7, 
                    Name = "Apple Music", 
                    Amount = 9.99m,
                    BillingCycle = "monthly",
                    DiscountRate = 0.0m,
                    StartDate = DateTime.Today.AddDays(-25),
                    Category = "Music" 
                },
                new Subscription { 
                    Id = 8, 
                    Name = "Microsoft 365", 
                    Amount = 6.99m,
                    BillingCycle = "yearly",
                    DiscountRate = 0.1m,
                    StartDate = DateTime.Today.AddDays(-180),
                    EndDate = DateTime.Today.AddDays(185),
                    Category = "Productivity" 
                },
                new Subscription { 
                    Id = 9, 
                    Name = "Dropbox", 
                    Amount = 11.99m,
                    BillingCycle = "yearly",
                    DiscountRate = 0.2m,
                    StartDate = DateTime.Today.AddDays(-365),
                    EndDate = DateTime.Today.AddDays(365),
                    Category = "Productivity" 
                },
                new Subscription { 
                    Id = 10, 
                    Name = "Canva Pro", 
                    Amount = 12.95m,
                    BillingCycle = "monthly",
                    DiscountRate = 0.0m,
                    StartDate = DateTime.Today.AddDays(-5),
                    Category = "Design" 
                }
            );
        }
    }
}
