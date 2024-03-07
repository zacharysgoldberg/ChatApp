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
		int userId = Context.User.GetUserId();
		bool isOnline = await _tracker.UserConnected(userId, Context.ConnectionId);

		if (isOnline)
			await Clients.Others.SendAsync("UserIsOnline", userId);

		int[] currentUsers = await _tracker.GetOnlineUsers();
		await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
	}

	public override async Task OnDisconnectedAsync(Exception exception)
	{
		int userId = Context.User.GetUserId();
		bool isOffline = await _tracker.UserDisconnected(userId, Context.ConnectionId);

		if (isOffline)
			await Clients.All.SendAsync("UserIsOffline", userId);

		await base.OnDisconnectedAsync(exception);
	}
}
