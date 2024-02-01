using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize] // (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)
public class UsersController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(UserManager<AppUser> userManager, IUserRepository userRepository,
        IMapper mapper)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _mapper = mapper;
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
        return await _userRepository.GetMemberAsync(username);
    }

    [HttpGet("members")] // /api/users/members
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetMembers()
    {
        return Ok( await _userRepository.GetMembersAsync());
    }

        
    [HttpGet("contacts/{contactUsername}")] // /api/users/contacts/test@test.com
    public async Task<ActionResult<ContactDTO>> GetContactByUsername(string contactUsername)
    {
        string username = User.GetUsername();

        var contacts = await _userRepository.GetContactsAsync(username);

        return contacts.FirstOrDefault(c => c.UserName == contactUsername);
    }    

    [HttpGet("contacts")] // /api/users/contacts
    public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContacts()
    {
        string username = User.GetUsername();

        return Ok(await _userRepository.GetContactsAsync(username));
    }
    // ===================================================
    // ================= POST Requests ===================
    // ===================================================

    [HttpPost("contacts")] // /api/users/contacts
    public async Task<ActionResult<ContactDTO>> AddContact(ContactDTO contactDTO)
    {
        string username = User.GetUsername();

        AppUser user = await _userRepository.GetUserByUsernameAsync(username);

        if(user == null)
            return NotFound();

        string contactUsername = contactDTO.UserName;

        if(user.Contacts.Find(c => c.UserName == contactUsername) != null)
            return BadRequest("Contact already exists in contact list");

        MemberDTO member = await _userRepository.GetMemberAsync(contactUsername);

        if(member == null)
            return NotFound();

        var contact = new Contact
        {
            UserName = member.UserName,
            Email = member.Email,
            Photo = member.Photo,
            LastActive = member.LastActive
        };

        user.Contacts.Add(contact);

        bool succeeded = await _userRepository.Update(user);

        if(succeeded)
            return _mapper.Map<ContactDTO>(contact);
            
        return BadRequest($"\nSomething went wrong with adding contact {contactUsername}");
    }

    // [Authorize(Roles = "Admin")]
    [HttpPost("delete/{username}")] // /api/users/delete/test@test.com
    public async Task<IActionResult> DeleteUser(string username)
    {
        AppUser user = await _userManager.FindByNameAsync(username);

        if(user == null)
            return NotFound();
        
        await _userManager.DeleteAsync(user);

        return Ok($"\nSuccessfully deleted user {username}.");
    }

    [HttpPost("contacts/delete/{contactUsername}")] // /api/users/contacts/delete/{test@test.com}
    public async Task<IActionResult> DeleteContact(string contactUsername)
    {
        string username = User.GetUsername();

        AppUser user = await _userRepository.GetUserByUsernameAsync(username);

        if(user == null)
            return NotFound();

        Contact contact = user.Contacts.Find(c => c.UserName == contactUsername);

        if(contact == null)
            return NotFound();

        user.Contacts.Remove(contact);

        bool succeeded = await _userRepository.Update(user);

        if(succeeded)
            return Ok($"\nSuccessfully removed {contactUsername} as a contact");
            
        return BadRequest($"\nSomething went wrong with adding contact {contactUsername}");
    }

    // ===================================================
    // ================= PUT Requests ====================
    // ===================================================

    [HttpPut] // /api/users
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
        string username = User.GetUsername();
            
        AppUser user = await _userManager.FindByNameAsync(username);

        if(user == null)
            return NotFound();
            
        _mapper.Map(memberUpdateDTO, user);

        await _userManager.UpdateAsync(user);

        return NoContent();
    }
}
