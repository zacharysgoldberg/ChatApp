using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
	private readonly IUserRepository _userRepository;
	private readonly IMessageRepository _messageRepository;
	private readonly IMapper _mapper;

	public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
	{
		_userRepository = userRepository;
		_messageRepository = messageRepository;
		_mapper = mapper;
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPost("thread")]
	public async Task<ActionResult<IEnumerable<MessageDTO>>> CreateMessageThread(CreateMessageDTO createMessageDTO)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO sender = await _userRepository.GetMemberAsync(usernameOrEmail);
		bool recipientExists = await _userRepository.UserIdExists(createMessageDTO.RecipientId);

		if (sender == null || !recipientExists)
			return NotFound();

		if (sender.Id == createMessageDTO.RecipientId)
			return BadRequest("Cannot create message thread with self");

		IEnumerable<MessageDTO> messageThread = await
			_messageRepository.CreateMessageThreadAsync(sender.Id, createMessageDTO.RecipientId);

		if (messageThread.Any())
			return Ok(messageThread);
		else
			return NoContent();
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPost]
	public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser sender = await _userRepository.GetUserAsync(usernameOrEmail);
		AppUser recipient = await _userRepository.GetUserByIdAsync(createMessageDTO.RecipientId);

		if (sender == null || recipient == null)
			return NotFound();

		if (sender.Id == createMessageDTO.RecipientId)
			return BadRequest("Cannot send messages to self");

		var message = new Message
		{
			Sender = sender,
			Recipient = recipient,
			Content = createMessageDTO.Content
		};

		if (await _messageRepository.CreateMessageAsync(message))
		{
			sender.LastActive = DateTime.UtcNow;
			IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(sender);

			if (!updateUserResult.Succeeded)
				return BadRequest("Failed to update user");

			return Ok(_mapper.Map<MessageDTO>(message));
		}

		return BadRequest("Failed to send message");
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("{recipientId}")]
	public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(int recipientId)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		return Ok(await _messageRepository.GetMessageThreadAsync(user.Id, recipientId));
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("contacts")]
	public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContactsWithMessageThread()
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		return Ok(await _messageRepository.GetContactsWithMessageThreadAsync(user.Id));
	}
}
