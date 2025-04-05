using System;
using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Service.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Subscription operations
    /// </summary>
    public class SubscriptionViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the subscription.
        /// </summary>
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
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the subscription is shared with others.
        /// </summary>
        public bool IsShared { get; set; } = false;

        /// <summary>
        /// Gets or sets the contact information for shared subscriptions.
        /// </summary>
        public string? ContactInfo { get; set; }
    }
}
