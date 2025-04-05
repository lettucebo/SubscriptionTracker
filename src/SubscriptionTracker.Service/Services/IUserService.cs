using SubscriptionTracker.Service.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SubscriptionTracker.Service.Services
{
    /// <summary>
    /// Interface for user management services.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets or creates a user based on claims.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal containing user information.</param>
        /// <returns>The user entity.</returns>
        Task<User> GetOrCreateUserAsync(ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// Gets a user by their Entra ID object ID.
        /// </summary>
        /// <param name="objectId">The Entra ID object ID.</param>
        /// <returns>The user entity if found, null otherwise.</returns>
        Task<User> GetUserByObjectIdAsync(string objectId);

        /// <summary>
        /// Gets the current user from the claims principal.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <returns>The current user entity.</returns>
        Task<User> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal);
    }
}
