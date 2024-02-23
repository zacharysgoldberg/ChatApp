using System.Numerics;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class GroupMessagesController : BaseApiController
{
	private readonly DataContext _context;
	private readonly IUserRepository _userRepository;
	private readonly IGroupMessageRepository _groupMessageRepository;
	private readonly IMapper _mapper;

	public GroupMessagesController(DataContext context, IUserRepository userRepository,
		IGroupMessageRepository groupMessageRepository, IMapper mapper)
	{
		_context = context;
		_userRepository = userRepository;
		_groupMessageRepository = groupMessageRepository;
		_mapper = mapper;
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPost("channel")]
	public async Task<ActionResult<IEnumerable<GroupMessageDTO>>> CreateGroupMessageChannel
		(CreateGroupMessageDTO createGroupMessageDTO)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO sender = await _userRepository.GetMemberAsync(usernameOrEmail);

		if (sender == null)
			return NotFound();

		IEnumerable<GroupMessageDTO> groupMessageChannel = await
			_groupMessageRepository.CreateGroupMessageChannelAsync(createGroupMessageDTO.ChannelId,
				sender.Id, createGroupMessageDTO.ContactIds);

		if (groupMessageChannel.Any())
			return Ok(groupMessageChannel);
		else
			return NoContent();
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPost]
	public async Task<ActionResult<IEnumerable<GroupMessageDTO>>> CreateGroupMessage
		(CreateGroupMessageDTO createGroupMessageDTO)
	{
		if (createGroupMessageDTO.ChannelId == null)
			return BadRequest("ChannelId is required");

		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO sender = await _userRepository.GetMemberAsync(usernameOrEmail);

		if (sender == null)
			return NotFound();

		var groupMessage = new GroupMessage
		{
			ChannelId = createGroupMessageDTO.ChannelId.Value,
			SenderId = sender.Id,
			Content = createGroupMessageDTO.Content
		};

		if (await _groupMessageRepository.CreateGroupMessageAsync(groupMessage))
			return Ok(_mapper.Map<GroupMessageDTO>(groupMessage));

		return BadRequest("Failed to send group message");
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("{channelId}")]
	public async Task<ActionResult<IEnumerable<GroupMessageDTO>>> GetGroupMessageChannel(Guid channelId)
	{
		GroupMessage result =
			await _context.GroupMessages.FirstOrDefaultAsync(gm => gm.ChannelId == channelId);

		if (result == null)
			return NotFound();

		return Ok(await _groupMessageRepository.GetGroupMessageChannelAsync(channelId));
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet]
	public async Task<ActionResult<IEnumerable<GroupMessageDTO>>> GetGroupMessageChannelsForUser()
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		return Ok(await _groupMessageRepository.GetGroupMessageChannelsForUserAsync(user.Id));
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("contacts/{channelId}")]
	public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContactsInGroupMessageChannel
		(Guid channelId)
	{
		IEnumerable<ContactDTO> contacts = await
			_groupMessageRepository.GetContactsInGroupMessageChannelAsync(channelId);

		if (contacts == null)
			return NotFound();

		return Ok(contacts);
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpDelete("{groupMessageId}")]
	public async Task<ActionResult> DeleteGroupMessage(int groupMessageId)
	{
		var message = await _context.GroupMessages.FirstOrDefaultAsync(gm => gm.Id == groupMessageId);
		bool deletedMessage = await _groupMessageRepository.DeleteGroupMessageAsync(message);

		if (deletedMessage)
			return NoContent();
		return BadRequest("Falied to delete group message");
	}
}
