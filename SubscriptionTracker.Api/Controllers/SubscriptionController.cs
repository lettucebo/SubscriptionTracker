using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        /// <param name="category">Optional subscription category to filter by.</param>
        /// <returns>A list of subscriptions with computed remaining days.</returns>
        [HttpGet]
        public async Task<IActionResult> GetSubscriptions([FromQuery] string category = null)
        {
            var query = _context.Subscriptions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category == category);
            }

            var subscriptions = await query.ToListAsync();

            var result = subscriptions.Select(s => new
            {
                s.Id,
                s.Name,
                s.Fee,
                s.PaymentDate,
                s.Category,
                RemainingDays = (s.PaymentDate - DateTime.Today).Days
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubscription(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
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
        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] Subscription subscription)
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubscription(int id, [FromBody] Subscription subscription)
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscription(int id)
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
        /// Checks if a subscription exists.
        /// </summary>
        /// <param name="id">The identifier of the subscription.</param>
        /// <returns>True if the subscription exists; otherwise, false.</returns>
        private bool SubscriptionExists(int id)
        {
            return _context.Subscriptions.Any(s => s.Id == id);
        }
    }
}
