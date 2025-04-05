using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public UserService(SubscriptionDbContext context)
        {
            _context = context;
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

            var user = await GetUserByObjectIdAsync(objectId);

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

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }

        /// <inheritdoc/>
        public async Task<User> GetUserByObjectIdAsync(string objectId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.ObjectId == objectId);
        }

        /// <inheritdoc/>
        public async Task<User> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            return await GetOrCreateUserAsync(claimsPrincipal);
        }
    }
}
