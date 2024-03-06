using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UsersController : BaseApiController
{
	private readonly IUserRepository _userRepository;
	private readonly IPhotoService _photoService;

	public UsersController(IUserRepository userRepository, IPhotoService photoService)
	{
		_userRepository = userRepository;
		_photoService = photoService;
	}
	// ==================== GET Requests ====================
	[Authorize(Roles = "Admin")]
	// [AllowAnonymous] // Development opnly
	[HttpGet("{usernameOrEmail}")] // /api/users/test@test.com
	public async Task<AppUser> GetUser(string usernameOrEmail)
	{
		return await _userRepository.GetUserAsync(usernameOrEmail);
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("members/{usernameOrEmail}")] // /api/users/members/test@test.com
	public async Task<ActionResult<MemberDTO>> GetMember(string usernameOrEmail)
	{
		return await _userRepository.GetMemberAsync(usernameOrEmail);
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("members")] // /api/users/members
	public async Task<ActionResult<IEnumerable<MemberDTO>>> GetMembers()
	{
		return Ok(await _userRepository.GetMembersAsync());
	}
	// ==================== POST Requests ====================
	[Authorize(Roles = "Member")]
	[HttpPost("update-photo")] // /api/users/update-photo
	public async Task<ActionResult<PhotoDTO>> UpdatePhoto(IFormFile file)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		var result = await _photoService.AddPhotoAsync(file);

		if (result.Error != null)
			return BadRequest(result.Error.Message);

		var photo = new Photo
		{
			Url = result.SecureUrl.AbsoluteUri,
			PublicId = result.PublicId
		};

		user.Photo = photo;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		return !updateUserResult.Succeeded ? BadRequest(updateUserResult.Errors) : Ok();
	}
	// ==================== PUT Requests ====================
	[Authorize(Roles = "Admin,Member")]
	[HttpPut("update-username")] // /api/users/update-username
	public async Task<ActionResult> UpdateUsername(MemberUpdateDTO memberUpdateDTO)
	{
		string username = memberUpdateDTO.UserName;
		bool usernameExists = await _userRepository.UsernameExistsAsync(username);

		if (usernameExists)
			return BadRequest($"Username \"{username}\" is already in use");

		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		user.UserName = username;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		return !updateUserResult.Succeeded ? BadRequest(updateUserResult.Errors) : NoContent();
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPut("update-email")] // /api/users/update-email
	public async Task<ActionResult> UpdateEmail(MemberUpdateDTO memberUpdateDTO)
	{
		string email = memberUpdateDTO.Email.ToLower();
		bool emailExists = await _userRepository.EmailExistsAsync(email);

		if (emailExists)
			return BadRequest($"Email \"{email}\" is already in use");

		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		user.Email = email;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		return !updateUserResult.Succeeded ? BadRequest(updateUserResult.Errors) : NoContent();
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPut("update-phone")] // /api/users/update-phone
	public async Task<ActionResult> UpdatePhoneNumber(MemberUpdateDTO memberUpdateDTO)
	{
		string phoneNumber = memberUpdateDTO.PhoneNumber;
		bool phoneNumberExists = await _userRepository.PhoneNumberExistsAsync(phoneNumber);

		if (phoneNumberExists)
			return BadRequest($"Phone Number \"{phoneNumber}\" is already in use");

		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		user.PhoneNumber = memberUpdateDTO.PhoneNumber;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		return !updateUserResult.Succeeded ? BadRequest(updateUserResult.Errors) : NoContent();
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPut("update-password")] // /api/users/reset-password
	public async Task<ActionResult> UpdatePassword(ChangePasswordDTO changePasswordDTO)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		IdentityResult updatePasswordResult = await _userRepository.UpdatePasswordAsync(user,
																									changePasswordDTO.CurrentPassword,
																									changePasswordDTO.NewPassword);

		return !updatePasswordResult.Succeeded ? BadRequest(updatePasswordResult.Errors) : NoContent();
	}

	// ==================== DELETE Requests ====================
	[Authorize(Roles = "Admin,Member")]
	[HttpDelete("delete-photo")]
	public async Task<ActionResult> DeletePhoto()
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);
		Photo photo = user.Photo;

		if (photo == null)
			return NotFound();

		if (photo.PublicId != null)
		{
			var result = await _photoService.DeletePhotoAsync(photo.PublicId);

			if (result.Error != null)
				return BadRequest(result.Error.Message);
		}

		user.Photo = null;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		return !updateUserResult.Succeeded ? BadRequest(updateUserResult.Errors) : Ok();
	}
}
