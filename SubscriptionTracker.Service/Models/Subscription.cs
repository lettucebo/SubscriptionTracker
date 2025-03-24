using System;
using System.ComponentModel.DataAnnotations;

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
        /// Gets or sets the fee of the subscription.
        /// </summary>
        [Required]
        public decimal Fee { get; set; }

        /// <summary>
        /// Gets or sets the payment date.
        /// </summary>
        [Required]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the category of the subscription.
        /// </summary>
        public string Category { get; set; } = "";
    }
}
