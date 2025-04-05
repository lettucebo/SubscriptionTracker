using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;
using SubscriptionTracker.Service.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionTracker.Api.Controllers
{
    /// <summary>
    /// REST API controller for managing subscription lifecycle operations
    /// </summary>
    /// <remarks>
    /// <para>This controller provides:</para>
    /// <list type="bullet">
    /// <item><description>Full CRUD operations for subscriptions</description></item>
    /// <item><description>Financial metric calculations</description></item>
    /// <item><description>Date management features</description></item>
    /// <item><description>Category-based filtering</description></item>
    /// </list>
    /// <para>Uses:</para>
    /// <list type="bullet">
    /// <item><description>Entity Framework Core for data access</description></item>
    /// <item><description>Repository pattern for data management</description></item>
    /// <item><description>SubscriptionCalculator for financial logic</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionDbContext _context;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the SubscriptionController class.
        /// </summary>
        /// <param name="context">The database context for subscriptions.</param>
        /// <param name="userService">The user service.</param>
        public SubscriptionController(SubscriptionDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all subscriptions with optional category filtering
        /// </summary>
        /// <param name="categoryId">Optional category identifier for filtering</param>
        /// <returns>
        /// List of subscriptions with calculated financial metrics and category details,
        /// ordered by remaining days in ascending order
        /// </returns>
        /// <response code="200">Successfully retrieved subscriptions list</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubscriptions(
            [FromQuery]
            int? categoryId = null)
        {
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            var query = _context.Subscriptions
                .Include(s => s.Category)
                .Where(s => s.UserId == currentUser.Id)
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
                s.EndDate,
                s.BillingCycle,
                s.DiscountRate,
                s.IsShared,
                s.ContactInfo,
                Category = new
                {
                    s.Category.Id,
                    s.Category.Name,
                    s.Category.Description
                },
                RemainingDays = SubscriptionCalculator.CalculateRemainingDays(
                    s.StartDate, s.EndDate, s.BillingCycle),
                EffectiveMonthlyPrice = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(
                    s.BillingCycle, s.Amount, s.DiscountRate)
            })
            .OrderBy(s => s.RemainingDays)
            .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single subscription by its unique identifier
        /// </summary>
        /// <param name="id">Subscription identifier (integer)</param>
        /// <returns>
        /// Complete subscription details including calculated metrics and category information
        /// </returns>
        /// <response code="200">Successfully retrieved subscription details</response>
        /// <response code="404">Subscription not found</response>
        /// <response code="400">Invalid identifier format</response>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubscription(
            [FromRoute]
            int id)
        {
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            var subscription = await _context.Subscriptions
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);
            if (subscription == null)
            {
                return NotFound();
            }
            var result = new
            {
                subscription.Id,
                subscription.Name,
                subscription.Amount,
                subscription.StartDate,
                subscription.EndDate,
                subscription.BillingCycle,
                subscription.DiscountRate,
                subscription.IsShared,
                subscription.ContactInfo,
                Category = new
                {
                    subscription.Category.Id,
                    subscription.Category.Name,
                    subscription.Category.Description
                },
                RemainingDays = SubscriptionCalculator.CalculateRemainingDays(
                    subscription.StartDate, subscription.EndDate, subscription.BillingCycle),
                EffectiveMonthlyPrice = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(
                    subscription.BillingCycle, subscription.Amount, subscription.DiscountRate)
            };

            return Ok(result);
        }

        /// <summary>
        /// Creates a new subscription entry
        /// </summary>
        /// <param name="subscription">
        /// Subscription data including amount, billing cycle, and optional category
        /// </param>
        /// <returns>Newly created subscription with calculated metrics</returns>
        /// <response code="201">Subscription created successfully</response>
        /// <response code="400">Invalid input data or validation failure</response>
        /// <response code="500">Database update error</response>
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

            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            // Associate the subscription with the current user
            subscription.UserId = currentUser.Id;

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var result = new
            {
                subscription.Id,
                subscription.Name,
                subscription.Amount,
                subscription.StartDate,
                subscription.EndDate,
                subscription.BillingCycle,
                subscription.DiscountRate,
                Category = subscription.Category == null ? null : new
                {
                    subscription.Category.Id,
                    subscription.Category.Name,
                    subscription.Category.Description
                },
                RemainingDays = SubscriptionCalculator.CalculateRemainingDays(
                    subscription.StartDate, subscription.EndDate, subscription.BillingCycle),
                EffectiveMonthlyPrice = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(
                    subscription.BillingCycle, subscription.Amount, subscription.DiscountRate)
            };

            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, result);
        }

        /// <summary>
        /// Updates an existing subscription's details
        /// </summary>
        /// <param name="id">Subscription identifier to update</param>
        /// <param name="subscription">Updated subscription data</param>
        /// <returns>
        /// No content response if successful, error details otherwise
        /// </returns>
        /// <response code="204">Subscription updated successfully</response>
        /// <response code="400">ID mismatch or invalid data</response>
        /// <response code="404">Subscription not found</response>
        /// <response code="409">Concurrency conflict</response>
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

            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            // Check if the subscription belongs to the current user
            var existingSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);

            if (existingSubscription == null)
            {
                return NotFound();
            }

            // Ensure the user ID doesn't change
            subscription.UserId = currentUser.Id;

            _context.Entry(subscription).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(await SubscriptionExists(id)))
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
        /// Permanently deletes a subscription
        /// </summary>
        /// <param name="id">Subscription identifier to delete</param>
        /// <returns>
        /// No content response if successful, error details otherwise
        /// </returns>
        /// <response code="204">Subscription deleted successfully</response>
        /// <response code="404">Subscription not found</response>
        /// <response code="500">Deletion operation failed</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSubscription(
            [FromRoute]
            int id)
        {
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            // Find the subscription and ensure it belongs to the current user
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);

            if (subscription == null)
            {
                return NotFound();
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Updates subscription start date and recalculates metrics
        /// </summary>
        /// <param name="id">Subscription identifier</param>
        /// <param name="startDate">New effective start date (UTC)</param>
        /// <returns>
        /// No content response if successful, error details otherwise
        /// </returns>
        /// <response code="204">Start date updated successfully</response>
        /// <response code="400">Invalid date format</response>
        /// <response code="404">Subscription not found</response>
        /// <response code="409">Concurrency conflict</response>
        [HttpPut("{id}/dates")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubscriptionDates(
            [FromRoute] int id,
            [FromBody] DateTime startDate)
        {
            // Get the current user
            var currentUser = await _userService.GetCurrentUserAsync(User);

            // Find the subscription and ensure it belongs to the current user
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);

            if (subscription == null)
            {
                return NotFound();
            }

            subscription.StartDate = DateOnly.FromDateTime(startDate);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SubscriptionExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Checks existence of a subscription in the database
        /// </summary>
        /// <param name="id">Subscription identifier to verify</param>
        /// <returns>
        /// True if subscription exists, false otherwise
        /// </returns>
        /// <remarks>
        /// Internal helper method used for concurrency checks
        /// </remarks>
        private async Task<bool> SubscriptionExists(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync(User);
            return await _context.Subscriptions.AnyAsync(s => s.Id == id && s.UserId == currentUser.Id);
        }
    }
}
