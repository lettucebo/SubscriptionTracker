using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Models;

namespace SubscriptionTracker.Data
{
    /// <summary>
    /// Database context for the subscription tracker application
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Subscriptions database set
        /// </summary>
        public DbSet<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Configure the model
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Subscription entity
            modelBuilder.Entity<Subscription>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.Cost)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        }

        /// <summary>
        /// Seed method to populate initial data
        /// </summary>
        public void SeedData()
        {
            if (!Subscriptions.Any())
            {
                Subscriptions.AddRange(
                    new Subscription 
                    { 
                        Name = "Netflix", 
                        Cost = 12.99m, 
                        PaymentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15) 
                    },
                    new Subscription 
                    { 
                        Name = "Spotify", 
                        Cost = 9.99m, 
                        PaymentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5) 
                    },
                    new Subscription 
                    { 
                        Name = "Amazon Prime", 
                        Cost = 8.99m, 
                        PaymentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22) 
                    }
                );
                SaveChanges();
            }
        }
    }
}