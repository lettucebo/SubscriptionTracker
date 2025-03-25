using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;

namespace SubscriptionTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly SubscriptionDbContext _context;

        public CategoryController(SubscriptionDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all active categories from the database
        /// </summary>
        /// <returns>
        /// ActionResult containing list of Category entities
        /// </returns>
        /// <response code="200">Returns the list of categories</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .Where(c => !c.IsDelete)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific category by its ID
        /// </summary>
        /// <param name="id">The ID of the category to retrieve</param>
        /// <returns>
        /// ActionResult with Category entity if found, NotFound otherwise
        /// </returns>
        /// <response code="200">Returns the requested category</response>
        /// <response code="404">If category is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        /// <summary>
        /// Creates a new category in the database
        /// </summary>
        /// <param name="category">Category entity to create</param>
        /// <returns>
        /// CreatedAtAction with the newly created category
        /// </returns>
        /// <response code="201">Returns the newly created category</response>
        /// <response code="400">If the category data is invalid</response>
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        /// <summary>
        /// Updates an existing category
        /// </summary>
        /// <param name="id">ID of the category to update</param>
        /// <param name="category">Updated Category entity</param>
        /// <returns>
        /// IActionResult indicating success or failure
        /// </returns>
        /// <response code="204">If update is successful</response>
        /// <response code="400">If ID mismatch</response>
        /// <response code="404">If category not found</response>
        /// <response code="409">If concurrency conflict occurs</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Soft-deletes a category by marking it as deleted
        /// </summary>
        /// <param name="id">ID of the category to delete</param>
        /// <returns>
        /// IActionResult indicating success or failure
        /// </returns>
        /// <response code="204">If deletion is successful</response>
        /// <response code="404">If category not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsDelete = true;
            category.DeleteAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if a category with the specified ID exists
        /// </summary>
        /// <param name="id">Category ID to check</param>
        /// <returns>
        /// True if exists, False otherwise
        /// </returns>
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
