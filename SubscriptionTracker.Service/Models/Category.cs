using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Service.Models
{
    /// <summary>
    /// Represents a subscription category.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        [Required]
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the description of the category.
        /// </summary>
        public string Description { get; set; } = "";
    }
}
