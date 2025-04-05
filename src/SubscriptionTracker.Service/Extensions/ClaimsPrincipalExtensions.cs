using System.Security.Claims;

namespace SubscriptionTracker.Service.Extensions
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Finds the first value of the specified claim type.
        /// </summary>
        /// <param name="principal">The claims principal.</param>
        /// <param name="claimType">The claim type.</param>
        /// <returns>The claim value if found, null otherwise.</returns>
        public static string? FindFirstValue(this ClaimsPrincipal principal, string claimType)
        {
            var claim = principal.FindFirst(claimType);
            return claim?.Value;
        }
    }
}
