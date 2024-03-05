using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
	private readonly PresenceTracker _tracker;
	private readonly IUserRepository _userRepository;

	public PresenceHub(PresenceTracker tracker, IUserRepository userRepository)
	{
		_tracker = tracker;
		_userRepository = userRepository;
	}

	public override async Task OnConnectedAsync()
	{
		string usernameOrEmail = Context.User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);

		await _tracker.UserConnected(user.Id, Context.ConnectionId);
		await Clients.Others.SendAsync("UserIsOnline", usernameOrEmail);

		int[] currentUsers = await _tracker.GetOnlineUsers();
		await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
	}

	public override async Task OnDisconnectedAsync(Exception exception)
	{
		string usernameOrEmail = Context.User.GetUsernameOrEmail();
		MemberDTO user = await _userRepository.GetMemberAsync(usernameOrEmail);

		await _tracker.UserDisconnected(user.Id, Context.ConnectionId);
		await Clients.Others.SendAsync("UserIsOffline", usernameOrEmail);

		int[] currentUsers = await _tracker.GetOnlineUsers();
		await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

		await base.OnDisconnectedAsync(exception);
	}
}
