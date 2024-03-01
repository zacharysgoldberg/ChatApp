using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace API.SignalR;

public class GroupMessageHub : Hub
{
	private readonly IGroupMessageRepository _groupMessageRepository;
	private readonly IUserRepository _userRepository;
	private readonly IMapper _mapper;
	private readonly IHubContext<PresenceHub> _presenceHub;

	public GroupMessageHub(IGroupMessageRepository groupMessageRepository,
		IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub)
	{
		_groupMessageRepository = groupMessageRepository;
		_userRepository = userRepository;
		_mapper = mapper;
		_presenceHub = presenceHub;
	}

	public override async Task OnConnectedAsync()
	{
		HttpContext httpContext = Context.GetHttpContext();

		string channelName = httpContext.Request.Query["channelName"];

		string contactIdsQueryString = httpContext.Request.Query["contactIds"];
		int[] contactIds = contactIdsQueryString.Split(',').Select(int.Parse).ToArray();

		string channelIdString = httpContext.Request.Query["channelId"];
		bool success = Guid.TryParse(channelIdString, out Guid channelId);

		if (!success)
			throw new HubException("Failed to parse channel id");

		await Groups.AddToGroupAsync(Context.ConnectionId, channelId.ToString());
		string usernameOrEmail = Context.User.GetUsernameOrEmail();
		MemberDTO sender = await _userRepository.GetMemberAsync(usernameOrEmail) ??
			throw new HubException("Not found");

		IEnumerable<GroupMessageDTO> groupMessageChannel = await
				_groupMessageRepository.GetGroupMessageChannelAsync(channelId);

		await Clients.Group(channelId.ToString())
			.SendAsync("ReceiveGroupMessageChannel", groupMessageChannel);
	}

	public override Task OnDisconnectedAsync(Exception exception)
	{
		return base.OnDisconnectedAsync(exception);
	}

	public async Task SendGroupMessage(CreateGroupMessageDTO createGroupMessageDTO)
	{
		if (createGroupMessageDTO.ChannelId == null)
			throw new HubException("Channel Id is required");

		string usernameOrEmail = Context.User.GetUsernameOrEmail();
		MemberDTO sender = await _userRepository.GetMemberAsync(usernameOrEmail) ??
			throw new HubException("Not found");

		var groupMessage = new GroupMessage
		{
			ChannelId = createGroupMessageDTO.ChannelId.Value,
			ChannelName = createGroupMessageDTO.ChannelName,
			SenderId = sender.Id,
			Content = createGroupMessageDTO.Content
		};

		if (await _groupMessageRepository.CreateGroupMessageAsync(groupMessage))
			await Clients.Group(createGroupMessageDTO.ChannelId.ToString())
				.SendAsync("NewGroupMessage", _mapper.Map<GroupMessageDTO>(groupMessage));
		else
			throw new HubException("Failed to send group message");
	}
}