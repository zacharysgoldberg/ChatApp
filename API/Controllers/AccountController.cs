using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        public AccountController(ITokenService             tokenService, 
                                 UserManager<AppUser>      userManager,
                                 IConfiguration            configuration)
        {
            _tokenService = tokenService;
            _userManager  = userManager;
            _config = configuration;
        }

        // Create new user
        [HttpPost("register")] // POST /api/account/register?username=dave&password=pwd
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            if(await UserExists(registerDto.Email.ToLower()))
                return BadRequest("Email is already in use");

            AppUser user = new AppUser
            {
                Email = registerDto.Email.ToLower(),
                UserName = registerDto.Email.ToLower(),
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                Console.WriteLine("----------------\n");
                foreach(var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
                    
                return BadRequest("User registration failed! Please check user details and try again.");
            }
            
            return Ok("User registration successful!");
        }

        // Sign user in and return a JWT and Refresh token
        [HttpPost("login")] // POST /api/account/login
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            AppUser user = await _userManager.FindByNameAsync(loginDTO.Username.ToLower());
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginDTO.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            string accessToken = _tokenService.GenerateAccessToken(claims);
            string refreshToken = _tokenService.GenerateRefreshToken();

            _ = int.TryParse(_config["JWT:RefreshTokenValidityInDays"],
                             out int refreshTokenValidityInDays);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            await _userManager.UpdateAsync(user);

            if(user != null && await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                return new UserDTO
                {
                    Username = user.UserName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }

            return Unauthorized("Invalid Username or Password");
        }

        // Generate the user a new JWT and Refresh token
        [HttpPost("refresh-token")] // POST /api/account/refresh-token
        public async Task<IActionResult> RefreshToken(UserDTO userDTO)
        {
            if (userDTO is null)
                return BadRequest("Invalid client request");

            string accessToken = userDTO.AccessToken;
            string refreshToken = userDTO.RefreshToken;

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            string username = principal.Identity.Name;

            AppUser user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            string newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            string newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            await _userManager.UpdateAsync(user);

            return Ok(new ObjectResult(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            }));
        }

        // Revoke user's Refresh token
        [Authorize]
        [HttpPost("revoke/{username}")] // POST /api/account/revoke/zach@zach.com
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Invalid username");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return Ok("Logged out successfully"); // Logout
        }

        // Logout and redirect user to login page
        // private async Task<ActionResult> Logout()
        // {

        // }

        private async Task<bool> UserExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
                return true;
            return false;
        }
    }
}
