using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        /// Gets or sets the color code in hex format
        /// </summary>
        [Required]
        [MaxLength(7)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color code format")]
        public string ColorCode { get; set; } = "#3A86FF";

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        [Required]
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the description of the category.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets whether the category is soft deleted.
        /// </summary>
        public bool IsDelete { get; set; } = false;

        /// <summary>
        /// Gets or sets the deletion timestamp.
        /// </summary>
        public DateTime? DeleteAt { get; set; }

        /// <summary>
        /// Gets or sets the user ID who created this category.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who created this category.
        /// </summary>
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
