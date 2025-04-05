using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Service.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for Category operations
    /// </summary>
    public class CategoryDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
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
    }
}
