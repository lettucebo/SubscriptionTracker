using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;
using SubscriptionTracker.Service.Services;

namespace SubscriptionTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly SubscriptionDbContext _context;
        private readonly IUserService _userService;

        public CategoryController(SubscriptionDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            return await _context.Categories
                .Where(c => !c.IsDelete && c.UserId == currentUser.Id)
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
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == currentUser.Id && !c.IsDelete);

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
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            // Associate the category with the current user
            category.UserId = currentUser.Id;

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
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != category.Id)
            {
                return BadRequest("ID mismatch");
            }

            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == currentUser.Id && !c.IsDelete);
            if (existingCategory == null)
            {
                return NotFound();
            }

            // Ensure the user ID doesn't change
            category.UserId = currentUser.Id;

            // Explicitly update allowed fields
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.ColorCode = category.ColorCode;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!(await CategoryExists(id)))
                {
                    return NotFound();
                }
                return StatusCode(409, "Concurrency conflict: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            await _context.Entry(existingCategory).ReloadAsync();
            return Ok(await _context.Categories.FindAsync(id));
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
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == currentUser.Id && !c.IsDelete);
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
        private async Task<bool> CategoryExists(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync(User);
            return await _context.Categories.AnyAsync(c => c.Id == id && c.UserId == currentUser.Id && !c.IsDelete);
        }
    }
}
