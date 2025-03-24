using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Service.Models;

namespace SubscriptionTracker.Service.Data
{
    /// <summary>
    /// Represents the database context for subscriptions.
    /// </summary>
    public class SubscriptionDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the subscriptions.
        /// </summary>
        public DbSet<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Initializes a new instance of the SubscriptionDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures the model and seeds initial data.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Seed 10 sample subscriptions
            modelBuilder.Entity<Subscription>().HasData(
                new Subscription { Id = 1, Name = "Netflix", Fee = 15.99m, PaymentDate = DateTime.Today.AddDays(10), Category = "Entertainment" },
                new Subscription { Id = 2, Name = "Spotify", Fee = 9.99m, PaymentDate = DateTime.Today.AddDays(5), Category = "Music" },
                new Subscription { Id = 3, Name = "Amazon Prime", Fee = 12.99m, PaymentDate = DateTime.Today.AddDays(20), Category = "Shopping" },
                new Subscription { Id = 4, Name = "Adobe Creative Cloud", Fee = 52.99m, PaymentDate = DateTime.Today.AddDays(15), Category = "Productivity" },
                new Subscription { Id = 5, Name = "HBO Max", Fee = 14.99m, PaymentDate = DateTime.Today.AddDays(9), Category = "Entertainment" },
                new Subscription { Id = 6, Name = "Disney+", Fee = 7.99m, PaymentDate = DateTime.Today.AddDays(8), Category = "Entertainment" },
                new Subscription { Id = 7, Name = "Apple Music", Fee = 9.99m, PaymentDate = DateTime.Today.AddDays(12), Category = "Music" },
                new Subscription { Id = 8, Name = "Microsoft 365", Fee = 6.99m, PaymentDate = DateTime.Today.AddDays(7), Category = "Productivity" },
                new Subscription { Id = 9, Name = "Dropbox", Fee = 11.99m, PaymentDate = DateTime.Today.AddDays(6), Category = "Productivity" },
                new Subscription { Id = 10, Name = "Canva Pro", Fee = 12.95m, PaymentDate = DateTime.Today.AddDays(11), Category = "Design" }
            );
        }
    }
}
