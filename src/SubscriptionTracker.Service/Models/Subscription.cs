using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SubscriptionTracker.Service.Models
{
    /// <summary>
    /// Represents a subscription service.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Gets or sets the unique identifier for the subscription.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the subscription.
        /// </summary>
        [Required]
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the billing amount.
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        [Obsolete("Fee has been replaced by Amount")]
        public decimal Fee
        {
            get => Amount;
            set => Amount = value;
        }

        /// <summary>
        /// Gets or sets the billing cycle (monthly/yearly).
        /// </summary>
        [Required]
        public string BillingCycle { get; set; } = "monthly";

        /// <summary>
        /// Gets or sets the discount rate for yearly payments.
        /// </summary>
        [Range(0, 1)]
        public decimal DiscountRate { get; set; }

        /// <summary>
        /// Gets or sets the subscription start date.
        /// </summary>
        [Required]
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets or sets the subscription end date (optional).
        /// </summary>
        public DateOnly? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the subscription.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category of the subscription.
        /// </summary>
        public Category Category { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the subscription is shared with others.
        /// </summary>
        public bool IsShared { get; set; } = false;

        /// <summary>
        /// Gets or sets the contact information for shared subscriptions.
        /// </summary>
        public string? ContactInfo { get; set; }

        /// <summary>
        /// Gets or sets the user ID who owns this subscription.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who owns this subscription.
        /// </summary>
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
