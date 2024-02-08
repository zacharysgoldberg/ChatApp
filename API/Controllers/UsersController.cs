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
    private readonly IMapper _mapper;

    public UsersController(UserManager<AppUser> userManager, IUserRepository userRepository,
        IContactService contactService, IMapper mapper)
    {
        _userManager    = userManager;
        _userRepository = userRepository;
        _contactService = contactService;
        _mapper         = mapper;
    }

    // ===================================================
    // ================= GET Requests ====================
    // ===================================================
    [AllowAnonymous] // Development Only
    // [Authorize(Roles="Admin")]
    [HttpGet] // /api/users
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        return Ok(await _userRepository.GetUsersAsync());
    }

    [AllowAnonymous] // Development Only
    // [Authorize(Roles="Admin")]
    [HttpGet("{username}")] // /api/users/test@test.com
    public async Task<AppUser> GetUserByUsername(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    [HttpGet("members/{username}")] // /api/users/members/test@test.com
    public async Task<ActionResult<MemberDTO>> GetMemberByUsername(string username)
    {
        return await _userRepository.GetMemberByUsernameAsync(username);
    }

    [HttpGet("members")] // /api/users/members
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetMembers()
    {
        return Ok(await _userRepository.GetMembersAsync());
    }

        
    [HttpGet("contacts/{contactId}")] // /api/users/contacts/2
    public async Task<ActionResult<ContactDTO>> GetContact(int contactId)
    {
        string username     = User.GetUsername();
        MemberDTO user      = await _userRepository.GetMemberByUsernameAsync(username);
        ContactDTO contact  = await _contactService.GetContactAsync(user.Id, contactId);

        if(contact == null)
            return NotFound();

        return contact;
    }    

    [HttpGet("contacts")] // /api/users/contacts/1
    public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContacts()
    {
        string username = User.GetUsername();
        MemberDTO user  = await _userRepository.GetMemberByUsernameAsync(username);

        return Ok(await _contactService.GetContactsAsync(user.Id));
    }

    // ===================================================
    // ================= POST Requests ===================
    // ===================================================
    [HttpPost("contacts")] // /api/users/contacts
    public async Task<ActionResult<MemberDTO>> AddContact([FromBody] 
        ContactUsernameDTO contactUsernameDTO)
    {
        string username = User.GetUsername();
        AppUser user    = await _userRepository.GetUserByUsernameAsync(username);

        if(user == null || contactUsernameDTO.Username == username)
            return BadRequest("You cannot add yourself as a contact");

        MemberDTO contact = await _userRepository.GetMemberByUsernameAsync(contactUsernameDTO.Username);

        if(contact == null)
            return BadRequest($"{contactUsernameDTO.Username} does not exist");

        bool userContactExists = await _contactService.UserContactExists(user.Id, contact.Id);

        if(contact == null || userContactExists)
            return BadRequest($"{contactUsernameDTO.Username} already exists in contact list");

        bool succeeded = await _contactService.AddContactAsync(user, contact.Id);

        if(succeeded)
            return contact;
            
        return BadRequest($"\nSomething went wrong with adding contact {contactUsernameDTO.Username}");
    }

    // [HttpPost("photo-upload")]
    // public async Task<ActionResult> AddPhoto()
    // {

    //     return NoContent();
    // }

    [HttpPost("contacts/delete/{contactId}")] // /api/users/contacts/delete/2
    public async Task<IActionResult> DeleteContact(int contactId)
    {
        string username = User.GetUsername();
        AppUser user    = await _userRepository.GetUserByUsernameAsync(username);

        if(user == null)
            return NotFound();

        bool succeeded = await _contactService.DeleteContactAsync(user, contactId);

        if(succeeded)
            return Ok();
            
        return BadRequest("\nSomething went wrong with removing contact");
    }

    // ===================================================
    // ================= PUT Requests ====================
    // ===================================================
    [HttpPut("update-username")] // /api/users
    public async Task<ActionResult> UpdateUsername(MemberUpdateDTO memberUpdateDTO)
    {
        bool usernameExists = await _userRepository.UsernameExistsAsync(memberUpdateDTO.UserName);

        if(usernameExists)
            return BadRequest($"Username \"{memberUpdateDTO.UserName}\" is already in use");

        string username = User.GetUsername();     
        AppUser user    = await _userManager.FindByNameAsync(username);

        if(user == null)
            return NotFound();
            
        // _mapper.Map(newUsername, user);
        user.UserName   = memberUpdateDTO.UserName;

        bool succeeded  = await _userRepository.UpdateAsync(user);
        
        if(!succeeded)
            return BadRequest("Failed to update username");

        return NoContent();
    }

    [HttpPut("update-email")] // /api/users
    public async Task<ActionResult> UpdateEmail(MemberUpdateDTO memberUpdateDTO)
    {
        bool emailExists = await _userRepository.EmailExistsAsync(memberUpdateDTO.Email);

        if(emailExists)
            return BadRequest($"Email \"{memberUpdateDTO.Email}\" is already in use");

        string username = User.GetUsername();
        AppUser user    = await _userManager.FindByNameAsync(username);

        if(user == null)
            return NotFound();
            
        // _mapper.Map(newUsername, user);
        user.Email      = memberUpdateDTO.Email;

        bool succeeded  = await _userRepository.UpdateAsync(user);
        
        if(!succeeded)
            return BadRequest("Failed to update email");

        return NoContent();
    }

    [HttpPut("reset-password")] // /api/users
    public async Task<ActionResult> UpdatePassword(ChangePasswordDTO changePasswordDTO)
    {
        string username = User.GetUsername();     
        AppUser user    = await _userManager.FindByNameAsync(username);

        if(user == null)
            return NotFound();
            
        // _mapper.Map(newUsername, user);

        await _userManager.ChangePasswordAsync(user, 
                                            changePasswordDTO.CurrentPassword, 
                                            changePasswordDTO.Password);

        return NoContent();
    }
}
