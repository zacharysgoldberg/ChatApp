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
		(CreateChannelDTO createChannelDTO)
	{
		int senderId = User.GetUserId();

		createChannelDTO.ContactIds.Add(senderId);

		IEnumerable<GroupMessageDTO> groupMessageChannel = await
			_groupMessageRepository.CreateGroupMessageChannelAsync(createChannelDTO.ChannelName,
				senderId, createChannelDTO.ContactIds);

		return Ok(groupMessageChannel);
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpPost]
	public async Task<ActionResult<GroupMessageDTO>> CreateGroupMessage
		(CreateChannelDTO createChannelDTO)
	{
		if (createChannelDTO.ChannelId == null)
			return BadRequest("ChannelId is required");

		int senderId = User.GetUserId();

		var groupMessage = new GroupMessage
		{
			ChannelId = createChannelDTO.ChannelId.Value,
			ChannelName = createChannelDTO.ChannelName,
			SenderId = senderId,
			Content = createChannelDTO.Content
		};

		if (await _groupMessageRepository.CreateGroupMessageAsync(groupMessage))
			return Ok(_mapper.Map<GroupMessageDTO>(groupMessage));

		return BadRequest("Failed to send group message");
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet]
	public async Task<ActionResult<IEnumerable<GroupMessageDTO>>> GetGroupMessageChannelNames()
	{
		int userId = User.GetUserId();

		return Ok(await _groupMessageRepository.GetGroupMessageChannelNamesAsync(userId));
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("{channelId}")]
	public async Task<ActionResult<IEnumerable<GroupMessageDTO>>> GetGroupMessageChannel(Guid channelId)
	{
		return Ok(await _groupMessageRepository.GetGroupMessageChannelAsync(channelId));
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpGet("contacts/{channelId}")]
	public async Task<ActionResult<IEnumerable<GroupMessageDTO>>>
		GetGroupMessageChannelContacts(Guid channelId)
	{
		return Ok(await _groupMessageRepository.GetGroupMessageChannelContactsAsync(channelId));
	}

	[Authorize(Roles = "Admin,Member")]
	[HttpDelete("{groupMessageId}")]
	public async Task<ActionResult> DeleteGroupMessage(int groupMessageId)
	{
		var message = await _context.GroupMessages.FirstOrDefaultAsync(gm => gm.Id == groupMessageId);
		bool deletedMessage = await _groupMessageRepository.DeleteGroupMessageAsync(message);

		if (deletedMessage)
			return NoContent();

		return BadRequest("Failed to delete group message");
	}
}
