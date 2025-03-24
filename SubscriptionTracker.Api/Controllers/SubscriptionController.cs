using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionTracker.Api.Controllers
{
    /// <summary>
    /// Controller for managing subscription services.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionDbContext _context;

        /// <summary>
        /// Initializes a new instance of the SubscriptionController class.
        /// </summary>
        /// <param name="context">The database context for subscriptions.</param>
        public SubscriptionController(SubscriptionDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all subscriptions with optional filtering by category and sorts by remaining days.
        /// </summary>
        /// <param name="categoryId">Optional category ID to filter subscriptions by.</param>
        /// <returns>A list of subscriptions with computed remaining days.</returns>
        /// <response code="200">Returns the list of subscriptions</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubscriptions(
            [FromQuery] 
            int? categoryId = null)
        {
            var query = _context.Subscriptions
                .Include(s => s.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == categoryId.Value);
            }

            var subscriptions = await query.ToListAsync();

            var result = subscriptions.Select(s => new
            {
                s.Id,
                s.Name,
                s.Amount,
                s.StartDate,
                s.BillingCycle,
                Category = new
                {
                    s.Category.Id,
                    s.Category.Name,
                    s.Category.Description
                },
                RemainingDays = s.RemainingDays,
                EffectiveMonthlyPrice = s.EffectiveMonthlyPrice
            })
            .OrderBy(s => s.RemainingDays)
            .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Gets a subscription by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the subscription.</param>
        /// <returns>The subscription if found; otherwise, NotFound.</returns>
        /// <response code="200">Returns the requested subscription</response>
        /// <response code="404">Subscription not found</response>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubscription(
            [FromRoute] 
            int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subscription == null)
            {
                return NotFound();
            }
            return Ok(subscription);
        }

        /// <summary>
        /// Creates a new subscription.
        /// </summary>
        /// <param name="subscription">The subscription object to create.</param>
        /// <returns>The created subscription.</returns>
        /// <response code="201">Returns the newly created subscription</response>
        /// <response code="400">Invalid input data</response>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSubscription(
            [FromBody] 
            Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
        }

        /// <summary>
        /// Updates an existing subscription.
        /// </summary>
        /// <param name="id">The identifier of the subscription to update.</param>
        /// <param name="subscription">The updated subscription object.</param>
        /// <returns>NoContent if update is successful; otherwise, NotFound or BadRequest.</returns>
        /// <response code="204">Update successful</response>
        /// <response code="400">ID mismatch</response>
        /// <response code="404">Subscription not found</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubscription(
            [FromRoute] 
            int id,
            [FromBody] 
            Subscription subscription)
        {
            if (id != subscription.Id)
            {
                return BadRequest("Subscription id mismatch.");
            }

            _context.Entry(subscription).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubscriptionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a subscription by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the subscription to delete.</param>
        /// <returns>NoContent if deletion is successful; otherwise, NotFound.</returns>
        /// <response code="204">Deletion successful</response>
        /// <response code="404">Subscription not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSubscription(
            [FromRoute] 
            int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Updates the start date for a subscription.
        /// </summary>
        /// <returns>True if the subscription exists; otherwise, false.</returns>
        /// <param name="id">The identifier of the subscription to update.</param>
        /// <param name="startDate">The new start date.</param>
        /// <returns>NoContent if update is successful; otherwise, NotFound.</returns>
        /// <response code="204">Update successful</response>
        /// <response code="404">Subscription not found</response>
        [HttpPut("{id}/dates")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubscriptionDates(
            [FromRoute] int id,
            [FromBody] DateTime startDate)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            subscription.StartDate = startDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubscriptionExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        private bool SubscriptionExists(int id)
        {
            return _context.Subscriptions.Any(s => s.Id == id);
        }
    }
}
