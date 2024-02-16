using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize] // (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)
public class UsersController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IPhotoService _photoService;

    public UsersController(UserManager<AppUser> userManager, IUserRepository userRepository,
        IPhotoService photoService)
    {
        _userManager    = userManager;
        _userRepository = userRepository;
        _photoService = photoService;
    }

    // ===================================================
    // ================= GET Requests ====================
    // ===================================================
    [AllowAnonymous] // Development Only
    // [Authorize(Roles="Admin")]
    [HttpGet("{usernameOrEmail}")] // /api/users/test@test.com
    public async Task<AppUser> GetUser(string usernameOrEmail)
    {
        return await _userRepository.GetUserAsync(usernameOrEmail);
    }

    [HttpGet("members/{usernameOrEmail}")] // /api/users/members/test@test.com
    public async Task<ActionResult<MemberDTO>> GetMember(string usernameOrEmail)
    {
        return await _userRepository.GetMemberAsync(usernameOrEmail);
    }

    [HttpGet("members")] // /api/users/members
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetMembers()
    {
        return Ok(await _userRepository.GetMembersAsync());
    }
    
    // ===================================================
    // ================= POST Requests ===================
    // ===================================================
    [HttpPost("update-photo")] // /api/users/update-photo
    public async Task<ActionResult<PhotoDTO>> UpdatePhoto(IFormFile file)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null)
            return NotFound();
        
        var result = await _photoService.AddPhotoAsync(file);

        if(result.Error != null) 
            return BadRequest(result.Error.Message);
        
        var photo = new Photo
        {
            Url      = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        
        user.Photo      = photo;
        bool succeeded  = await _userRepository.UpdateAsync(user);

        if(!succeeded)
            return BadRequest("Failed to add photo");

        return Ok();
    }

    // ===================================================
    // ================= PUT Requests ====================
    // ===================================================
    [HttpPut("update-username")] // /api/users/update-username
    public async Task<ActionResult> UpdateUsername(MemberUpdateDTO memberUpdateDTO)
    {   
        string username     = memberUpdateDTO.UserName;
        bool usernameExists = await _userRepository.UsernameExistsAsync(username);

        if(usernameExists)
            return BadRequest($"Username \"{username}\" is already in use");

        string usernameOrEmail  = User.GetUsernameOrEmail();     
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null)
            return NotFound();
            
        user.UserName   = username;
        bool succeeded  = await _userRepository.UpdateAsync(user);
        
        if(!succeeded)
            return BadRequest("Failed to update username");
            
        return NoContent();
    }

    [HttpPut("update-email")] // /api/users/update-email
    public async Task<ActionResult> UpdateEmail(MemberUpdateDTO memberUpdateDTO)
    {
        string email        = memberUpdateDTO.Email.ToLower();
        bool emailExists    = await _userRepository.EmailExistsAsync(email);

        if(emailExists)
            return BadRequest($"Email \"{email}\" is already in use");

        string usernameOrEmail  = User.GetUsernameOrEmail();
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null)
            return NotFound();
            
        user.Email      = email;
        bool succeeded  = await _userRepository.UpdateAsync(user);
        
        if(!succeeded)
            return BadRequest("Failed to update email");
        
        return NoContent();
    }

    [HttpPut("update-password")] // /api/users/reset-password
    public async Task<ActionResult> UpdatePassword(ChangePasswordDTO changePasswordDTO)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();     
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null)
            return NotFound();
            
        // DebugUtil.PrintDebug(user);
        // _mapper.Map(newUsername, user);

        await _userManager.ChangePasswordAsync(user, 
                                            changePasswordDTO.CurrentPassword, 
                                            changePasswordDTO.Password);

        return NoContent();
    }

    // ===================================================
    // ================= DELETE Requests ====================
    // ===================================================
    [HttpDelete("delete-photo")]
    public async Task<ActionResult> DeletePhoto()
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);
        Photo photo             = user.Photo;

        if(photo == null)
            return NotFound();
        
        if(photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);

            if(result.Error != null)
                return BadRequest(result.Error.Message);
        }

        user.Photo      = null;
        bool succeeded  = await _userRepository.UpdateAsync(user);

        if(!succeeded)
            return BadRequest("Failed to delete photo");
        
        return Ok();
    }
}
