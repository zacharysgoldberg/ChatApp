using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize] // (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)
    public class UsersController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet] // GET /api/users
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            
            return users;
        }
        
        [HttpGet("{id}")] // GET /api/users/1
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(user => user.Id == id);

            return user;
        }

        // [Authorize(Roles = "Admin")]
        [HttpPost("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {

            var user = await _userManager.Users.FirstOrDefaultAsync(user => user.Id == id);

            await _userManager.DeleteAsync(user);

            return Ok($"Deleted {user.UserName} successfully.");
        }
    }
}