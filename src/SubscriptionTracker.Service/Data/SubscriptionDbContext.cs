using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Service.Models;
using System;

namespace SubscriptionTracker.Service.Data
{
    /// <summary>
    /// Represents the database context for subscriptions.
    /// </summary>
    public class SubscriptionDbContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Category> Categories { get; set; }

        public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // This method is called if the context is used without being configured
            // The actual connection string is provided in Program.cs from user secrets
            // We don't provide a fallback here to avoid hardcoding sensitive information
            // Design-time tools should use the connection string from user secrets
        }

        /// <summary>
        /// Configures the model and seeds initial data.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.ColorCode)
                    .HasMaxLength(7)
                    .HasDefaultValue("#3A86FF");

                entity.Property(c => c.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(c => c.Description)
                    .HasMaxLength(200);

                // Soft delete configuration
                entity.Property(c => c.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(c => c.DeleteAt)
                    .IsRequired(false);
            });

            // Configure Subscription entity
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.Property(s => s.Amount)
                    .HasColumnType("decimal(18,2)");

                // Configure the obsolete Fee property to avoid warnings
                entity.Property(s => s.Fee)
                    .HasColumnType("decimal(18,2)");

                entity.Property(s => s.BillingCycle)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(s => s.DiscountRate)
                    .HasColumnType("decimal(3,2)")
                    .HasDefaultValue(0m);

                entity.Property(s => s.IsShared)
                    .HasDefaultValue(false);

                entity.Property(s => s.ContactInfo)
                    .HasMaxLength(500)
                    .IsRequired(false);

                // Configure DateOnly properties
                entity.Property(s => s.StartDate)
                    .HasConversion(
                        d => d.ToDateTime(TimeOnly.MinValue),
                        d => DateOnly.FromDateTime(d))
                    .HasColumnType("date");  // 明確指定使用 SQL Server 的 date 型態

                entity.Property(s => s.EndDate)
                    .HasConversion(
                        d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        d => d.HasValue ? DateOnly.FromDateTime(d.Value) : null)
                    .HasColumnType("date");  // 明確指定使用 SQL Server 的 date 型態
            });

            // Seed data removed to allow manual data entry
        }
    }
}
