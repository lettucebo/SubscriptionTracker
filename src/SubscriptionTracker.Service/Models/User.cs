using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Service.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique object ID from Entra ID.
        /// </summary>
        [Required]
        public string ObjectId { get; set; } = "";

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        [Required]
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required]
        public string Email { get; set; } = "";

        /// <summary>
        /// Gets or sets the date when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the collection of subscriptions owned by this user.
        /// </summary>
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        /// <summary>
        /// Gets or sets the collection of categories created by this user.
        /// </summary>
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
