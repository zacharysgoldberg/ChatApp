using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
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
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public AccountController(ITokenService             tokenService, 
                                 UserManager<AppUser>      userManager,
                                 IUserRepository           userRepository,
                                 IConfiguration            configuration)
        {
            _tokenService   = tokenService;
            _userManager    = userManager;
            _userRepository = userRepository;
            _config         = configuration;
        }

        // Create new user
        [HttpPost("register")] // POST /api/account/register?username=dave&password=pwd
        public async  Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await _userRepository.EmailExistsAsync(registerDTO.Email.ToLower()))
                return BadRequest("\nEmail is already in use");

            AppUser user = new AppUser
            {
                Email           = registerDTO.Email.ToLower(),
                UserName        = registerDTO.Email.ToLower(),
                SecurityStamp   = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
                return BadRequest("\nUser registration failed! Check user details and try again");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, registerDTO.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            string accessToken  = _tokenService.GenerateAccessToken(claims);
            string refreshToken = _tokenService.GenerateRefreshToken();
            bool succeeded      = int.TryParse(_config["JWT:RefreshTokenValidityInDays"],
                                                out int refreshTokenValidityInDays);
            
            if(!succeeded)
                return BadRequest("Failed to convert refresh token validity into a 32-bit signed integer");

            user.RefreshToken           = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            await _userManager.UpdateAsync(user);

            return Ok(new UserDTO
            {
                Username        = user.UserName,
                AccessToken     = accessToken,
                RefreshToken    = refreshToken
            });
        }

        // Sign user in and return a JWT and Refresh token
        [HttpPost("login")] // POST /api/account/login
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            if (loginDTO.Username is null || loginDTO.Password is null)
                return Unauthorized("\nInvalid Username or Password");
                
            AppUser user = await _userRepository.GetUserAsync(loginDTO.Username);

            if(user == null)
            {   
                return Unauthorized("\nInvalid Username or Password");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginDTO.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            string accessToken  = _tokenService.GenerateAccessToken(claims);
            string refreshToken = _tokenService.GenerateRefreshToken();
            bool succeeded      = int.TryParse(_config["JWT:RefreshTokenValidityInDays"],
                                                out int refreshTokenValidityInDays);
            
            if(!succeeded)
                return BadRequest();

            if(await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                user.RefreshToken           = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                user.LastActive             = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);
                
                return Ok(new UserDTO
                {
                    Username        = user.UserName,
                    AccessToken     = accessToken,
                    RefreshToken    = refreshToken
                });
            }

            return Unauthorized("\nInvalid Username or Password");
        }

        // Generate the user a new JWT and Refresh token
        [HttpPost("refresh-token")] // POST /api/account/refresh-token
        public async Task<IActionResult> RefreshToken(UserDTO userDTO)
        {
            if (userDTO is null)
                return BadRequest("\nInvalid client request");

            string accessToken          = userDTO.AccessToken;
            string refreshToken         = userDTO.RefreshToken;
            var principal               = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            string usernameOrEmail      = principal.Identity.Name;

            AppUser user                = await _userRepository.GetUserAsync(usernameOrEmail);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("\nInvalid access token or refresh token");                

            string newAccessToken   = _tokenService.GenerateAccessToken(principal.Claims);
            string newRefreshToken  = _tokenService.GenerateRefreshToken();
            user.RefreshToken       = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return Ok(new UserDTO()
            {
                Username        = user.UserName,
                AccessToken     = newAccessToken,
                RefreshToken    = newRefreshToken
            });
        }

        // Revoke user's Refresh token
        [Authorize]
        [HttpPost("revoke/{usernameOrEmail}")] // POST /api/account/revoke/johndoe@domain.com
        public async Task<IActionResult> Revoke(string usernameOrEmail)
        {
            AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

            if (user == null) 
                return BadRequest("\nInvalid username");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        // {
        //     if (!ModelState.IsValid)
        //         return View(forgotPasswordModel);

        //     var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
        //     if (user == null)
        //         return RedirectToAction(nameof(ForgotPasswordConfirmation));

        //     var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //     var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);

        //     var message = new Message(new string[] { user.Email }, "Reset password token", callback, null);
        //     await _emailSender.SendEmailAsync(message);

        //     return RedirectToAction(nameof(ForgotPasswordConfirmation));
        // }
      

        [HttpPost("reset-password")] // /api/account/reset-password
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            string usernameOrEmail  = User.GetUsernameOrEmail();
            AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

            if(user == null)
                return NotFound();
                
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, 
                                                                        resetPasswordDTO.Password);
            if(!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest();
            }
            return NoContent();
        }
    }
}
