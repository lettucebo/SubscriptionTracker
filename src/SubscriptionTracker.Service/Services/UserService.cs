using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Principal;
using SubscriptionTracker.Service.Extensions;

namespace SubscriptionTracker.Service.Services
{
    /// <summary>
    /// Service for user management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly SubscriptionDbContext _context;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="cache">The memory cache for storing user data.</param>
        public UserService(SubscriptionDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <inheritdoc/>
        public async Task<User> GetOrCreateUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null || !claimsPrincipal.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var objectId = claimsPrincipal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                ?? claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(objectId))
            {
                throw new UnauthorizedAccessException("User object ID not found in claims");
            }

            // Try to get the existing user first
            var user = await GetUserByObjectIdAsync(objectId);

            // If user doesn't exist, create a new one
            if (user == null)
            {
                // Create a new user
                var name = claimsPrincipal.FindFirstValue("name")
                    ?? claimsPrincipal.FindFirstValue(ClaimTypes.Name)
                    ?? "Unknown User";

                var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email)
                    ?? claimsPrincipal.FindFirstValue("preferred_username")
                    ?? "unknown@example.com";

                user = new User
                {
                    ObjectId = objectId,
                    DisplayName = name,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Cache the newly created user
                    var cacheKey = $"User_ObjectId_{objectId}";
                    _cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    // If we get a duplicate key error, the user was created by another concurrent request
                    // Just retrieve the existing user and invalidate the cache
                    _context.Entry(user).State = EntityState.Detached;
                    
                    // Remove from cache to ensure fresh data
                    var cacheKey = $"User_ObjectId_{objectId}";
                    _cache.Remove(cacheKey);
                    
                    user = await GetUserByObjectIdAsync(objectId);

                    if (user == null)
                    {
                        // This should not happen, but just in case
                        throw new InvalidOperationException("Failed to retrieve user after duplicate key error", ex);
                    }
                }
            }

            return user;
        }

        /// <inheritdoc/>
        public async Task<User> GetUserByObjectIdAsync(string objectId)
        {
            // Check cache first
            var cacheKey = $"User_ObjectId_{objectId}";
            if (_cache.TryGetValue(cacheKey, out User cachedUser))
            {
                return cachedUser;
            }

            // Make sure to use AsNoTracking to avoid conflicts with tracked entities
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ObjectId == objectId);

            // Cache the user for 5 minutes
            if (user != null)
            {
                _cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
            }

            return user;
        }

        /// <inheritdoc/>
        public async Task<User> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                return await GetOrCreateUserAsync(claimsPrincipal);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Error getting current user: {ex.Message}");
                throw;
            }
        }
    }
}
