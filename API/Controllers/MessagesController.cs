using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API;

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

	[Authorize(Roles = "Member")]
	[HttpPost("thread")]
	public async Task<ActionResult<IEnumerable<MessageDTO>>> CreateMessageThread(CreateMessageDTO createMessageDTO)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser sender = await _userRepository.GetUserAsync(usernameOrEmail);
		AppUser recipient = await _userRepository.GetUserByIdAsync(createMessageDTO.RecipientId);

		if (sender == null || recipient == null)
			return NotFound();

		if (sender.Id == createMessageDTO.RecipientId)
			return BadRequest("Cannot create message thread with self");

		IEnumerable<MessageDTO> messageThread = await _messageRepository.AddMessageThreadAsync(sender.Id, recipient.Id);

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

		_messageRepository.AddMessageAsync(message);

		if (await _messageRepository.SaveAllAsync())
			return Ok(_mapper.Map<MessageDTO>(message));

		return BadRequest("Failed to send message");
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet]
	public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser(
			[FromQuery] MessageParams messageParams)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		messageParams.Id = user.Id;
		PagedList<MessageDTO> messages = await _messageRepository.GetMessagesAsync(messageParams);

		Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

		return messages;
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("thread/{recipientId}")]
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
