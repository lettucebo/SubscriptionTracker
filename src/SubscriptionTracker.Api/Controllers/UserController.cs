using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionTracker.Service.Services;
using System.Threading.Tasks;

namespace SubscriptionTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets the current user's profile.
        /// </summary>
        /// <returns>The current user's profile.</returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            
            return Ok(new
            {
                user.Id,
                user.DisplayName,
                user.Email,
                user.CreatedAt
            });
        }
    }
}
