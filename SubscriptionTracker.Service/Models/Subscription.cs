using System;
using System.ComponentModel.DataAnnotations;
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
    public decimal Fee { 
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
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the subscription end date (optional).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the subscription.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category of the subscription.
        /// </summary>
        public Category Category { get; set; } = null!;

    }
}
