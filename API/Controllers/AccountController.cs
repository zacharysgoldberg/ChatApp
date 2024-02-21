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
		private readonly UserManager<AppUser> _userManager;
		private readonly IUserRepository _userRepository;
		private readonly IAuthService _authService;

		public AccountController(UserManager<AppUser> userManager, IUserRepository userRepository,
			IAuthService authService)
		{
			_userManager = userManager;
			_userRepository = userRepository;
			_authService = authService;
		}

		[HttpPost("register")] // POST /api/account/register?username=dave&password=pwd
		public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
		{
			if (await _userRepository.EmailExistsAsync(registerDTO.Email.ToLower()))
				return BadRequest("\nEmail is already in use");

			var (newUser, msg) = await _authService.Register(registerDTO, "Member");

			if (newUser == null)
				return BadRequest(msg);

			return Ok(newUser);
		}

		[HttpPost("login")] // POST /api/account/login
		public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
		{
			if (loginDTO.Username is null || loginDTO.Password is null)
				return Unauthorized("\nInvalid Username or Password");

			var (user, msg) = await _authService.Authenticate(loginDTO);

			if (user == null)
				return BadRequest(msg);

			return Ok(user);
		}

		[HttpPost("refresh-token")] // POST /api/account/refresh-token
		public async Task<ActionResult<UserDTO>> RefreshToken(UserDTO userDTO)
		{
			if (userDTO is null)
				return BadRequest("\nInvalid client request");

			var (user, msg) = await _authService.RefreshToken(userDTO);

			if (user == null)
				return BadRequest(msg);

			return Ok(user);
		}

		// Revoke user's Refresh token
		[Authorize(Roles = "Admin")]
		[HttpPost("revoke/{usernameOrEmail}")] // POST /api/account/revoke/johndoe@domain.com
		public async Task<IActionResult> Revoke(string usernameOrEmail)
		{
			AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

			if (user == null)
				return BadRequest("\nInvalid username");

			user.RefreshToken = null;
			bool updatedUser = await _userRepository.UpdateUserAsync(user);

			if (!updatedUser)
				return BadRequest("Failed to update user account");

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
			string usernameOrEmail = User.GetUsernameOrEmail();
			AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

			if (user == null)
				return NotFound();

			var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.Token,
																																	resetPasswordDTO.Password);
			if (!resetPassResult.Succeeded)
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
