using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
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

	[HttpGet("thread/{recipientId}")]
	public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(int recipientId)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		return Ok(await _messageRepository.GetMessageThreadAsync(user.Id, recipientId));
	}

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
