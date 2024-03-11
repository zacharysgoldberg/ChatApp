using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController : BaseApiController
{
	private readonly UserManager<AppUser> _userManager;
	private readonly IUserRepository _userRepository;
	private readonly IAuthService _authService;
	private readonly IEmailService _emailService;

	public AccountController(UserManager<AppUser> userManager, IUserRepository userRepository,
													IAuthService authService, IEmailService emailService)
	{
		_userManager = userManager;
		_userRepository = userRepository;
		_authService = authService;
		_emailService = emailService;
	}

	[HttpPost("register")] // POST /api/account/register?username=dave&password=pwd
	public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
	{
		if (await _userRepository.EmailExistsAsync(registerDTO.Email.ToLower()))
			return BadRequest("Email is already in use");

		var (newUser, error) = await _authService.Register(registerDTO, "Member");

		return newUser == null ? BadRequest(error) : Ok(newUser);
	}

	[HttpPost("login")] // POST /api/account/login
	public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
	{
		if (loginDTO.Username is null || loginDTO.Password is null)
			return Unauthorized("Invalid Username or Password");

		var (user, error) = await _authService.Authenticate(loginDTO);

		return user == null ? BadRequest(error) : Ok(user);
	}

	[HttpPost("refresh-token")] // POST /api/account/refresh-token
	public async Task<ActionResult<UserDTO>> RefreshToken(UserDTO userDTO)
	{
		if (userDTO is null)
			return BadRequest("Invalid client request");

		var (user, error) = await _authService.RefreshToken(userDTO);

		return user == null ? Unauthorized(error) : Ok(user);
	}

	// Revoke user's Refresh token
	[Authorize(Roles = "Admin")]
	[HttpPost("revoke/{usernameOrEmail}")] // POST /api/account/revoke/johndoe@domain.com
	public async Task<IActionResult> Revoke(string usernameOrEmail)
	{
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null)
			return BadRequest("Invalid username");

		user.RefreshToken = null;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		return updateUserResult.Succeeded ? BadRequest(updateUserResult.Errors) : Ok();
	}

	[HttpPost("forgot-password")] // /api/account/forgot-password
	public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
	{
		AppUser user = await _userManager.FindByEmailAsync(forgotPasswordDTO.Email);

		if (user == null)
			return NotFound();

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);

		DebugUtil.PrintDebug(token);

		var callback = $"https://localhost:4200/reset-password?email={user.Email}&token={token}";

		var message = new EmailMessage(new Dictionary<string, string>() { { user.UserName, user.Email } },
																	"Reset Password - ChatApp",
																	callback);

		await _emailService.SendEmailAsync(message);

		return Ok(new { token });
	}

	[HttpPost("reset-password")] // /api/account/reset-password
	public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
	{
		AppUser user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);

		if (user == null)
			return NotFound();

		var resetPassResult = await _userManager.ResetPasswordAsync(user,
																																resetPasswordDTO.Token,
																																resetPasswordDTO.Password);
		if (!resetPassResult.Succeeded)
			return BadRequest(resetPassResult.Errors);

		return Ok();
	}
}
