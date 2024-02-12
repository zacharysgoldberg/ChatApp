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
    private readonly IContactService _contactService;
    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;

    public UsersController(UserManager<AppUser> userManager, IUserRepository userRepository,
        IContactService contactService, IPhotoService photoService, IMapper mapper)
    {
        _userManager    = userManager;
        _userRepository = userRepository;
        _contactService = contactService;
        _photoService = photoService;
        _mapper = mapper;
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

        
    [HttpGet("contacts/{contactId}")] // /api/users/contacts/2
    public async Task<ActionResult<ContactDTO>> GetContact(int contactId)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        MemberDTO user          = await _userRepository.GetMemberAsync(usernameOrEmail);
        ContactDTO contact      = await _contactService.GetContactAsync(user.Id, contactId);

        if(contact == null)
            return BadRequest("Failed to update username");

        return contact;
    }    

    [HttpGet("contacts")] // /api/users/contacts/1
    public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContacts()
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        MemberDTO user          = await _userRepository.GetMemberAsync(usernameOrEmail);

        return Ok(await _contactService.GetContactsAsync(user.Id));
    }

    // ===================================================
    // ================= POST Requests ===================
    // ===================================================
    [HttpPost("contacts")] // /api/users/contacts
    public async Task<ActionResult<MemberDTO>> AddContact([FromBody] 
        ContactUsernameDTO contactUsernameDTO)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null || contactUsernameDTO.Username == usernameOrEmail)
            return BadRequest("You cannot add yourself as a contact");

        MemberDTO contact = await _userRepository.GetMemberAsync(contactUsernameDTO.Username);

        if(contact == null)
            return BadRequest($"{contactUsernameDTO.Username} does not exist");

        bool userContactExists = await _contactService.UserContactExists(user.Id, contact.Id);

        if(contact == null || userContactExists)
            return BadRequest($"{contactUsernameDTO.Username} already exists in contact list");

        bool succeeded = await _contactService.AddContactAsync(user, contact.Id);

        if(!succeeded)
            return BadRequest($"\nFailed to add contact {contactUsernameDTO.Username}");
            
        return contact;
    }

    [HttpPost("contacts/delete/{contactId}")] // /api/users/contacts/delete/2
    public async Task<IActionResult> DeleteContact(int contactId)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null)
            return NotFound();

        bool succeeded = await _contactService.DeleteContactAsync(user, contactId);

        if(!succeeded)
            return BadRequest("\nFailed to remove contact");
            
        return Ok();
    }

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
        
        var photo       = new Photo
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

    [HttpPut("reset-password")] // /api/users/reset-password
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
