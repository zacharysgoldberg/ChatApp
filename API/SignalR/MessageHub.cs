using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
	private readonly IMessageRepository _messageRepository;
	private readonly IGroupMessageRepository _groupMessageRepository;
	private readonly IUserRepository _userRepository;
	private readonly IMapper _mapper;

	public MessageHub(IMessageRepository messageRepository,
		IGroupMessageRepository groupMessageRepository, IUserRepository userRepository,
		IMapper mapper)
	{
		_messageRepository = messageRepository;
		_groupMessageRepository = groupMessageRepository;
		_userRepository = userRepository;
		_mapper = mapper;
	}

	public override async Task OnConnectedAsync()
	{
		var httpContext = Context.GetHttpContext();
		int recipientId;
		bool success = int.TryParse(httpContext.Request.Query["recipientId"], out recipientId);

		if (!success)
			throw new HubException("Failed to parse recipient id");

		string usernameOrEmail = Context.User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);
		MemberDTO recipient = await _userRepository.GetMemberByIdAsync(recipientId);

		if (user == null || recipient == null)
			throw new HubException("Not found");

		var groupName = GetGroupName(Context.User.GetUsernameOrEmail(), recipient.UserName);
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

		IEnumerable<MessageDTO> messages = await _messageRepository.CreateMessageThreadAsync
			(user.Id, recipient.Id);

		await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
	}

	public override Task OnDisconnectedAsync(Exception exception)
	{
		return base.OnDisconnectedAsync(exception);
	}

	public async Task SendMessage(CreateMessageDTO createMessageDTO)
	{
		string usernameOrEmail = Context.User.GetUsernameOrEmail();
		AppUser sender = await _userRepository.GetUserAsync(usernameOrEmail);
		AppUser recipient = await _userRepository.GetUserByIdAsync(createMessageDTO.RecipientId);

		if (sender == null || recipient == null)
			throw new HubException("Not found");

		if (sender.Id == createMessageDTO.RecipientId)
			throw new HubException("Cannot send messages to self");

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
				throw new HubException("Failed to update user");

			var group = GetGroupName(sender.UserName, recipient.UserName);
			await Clients.Group(group).SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
		}

		throw new HubException("Failed to send message");
	}

	private static string GetGroupName(string caller, string other)
	{
		var stringCompare = string.CompareOrdinal(caller, other) < 0;

		return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
	}
}