namespace API.SignalR;

public class PresenceTracker
{
	private static readonly Dictionary<int, List<string>> _onlineUsers = new();

	public Task<bool> UserConnected(int userId, string connectionId)
	{
		bool isOnline = false;

		lock (_onlineUsers)
		{
			if (_onlineUsers.ContainsKey(userId))
			{
				_onlineUsers[userId].Add(connectionId);
			}
			else
			{
				_onlineUsers.Add(userId, new List<string> { connectionId });
				isOnline = true;
			}
		}
		return Task.FromResult(isOnline);
	}

	public Task<bool> UserDisconnected(int userId, string connectionId)
	{
		bool isOffline = false;

		lock (_onlineUsers)
		{
			if (!_onlineUsers.ContainsKey(userId))
				return Task.FromResult(isOffline);

			_onlineUsers[userId].Remove(connectionId);

			if (_onlineUsers[userId].Count == 0)
			{
				_onlineUsers.Remove(userId);
				isOffline = true;
			}
		}

		return Task.FromResult(isOffline);
	}

	public Task<int[]> GetOnlineUsers()
	{
		int[] onlineUserIds;

		lock (_onlineUsers)
		{
			onlineUserIds = _onlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
		}

		return Task.FromResult(onlineUserIds);
	}

	public static Task<List<string>> GetConnectionsForUser(int userId)
	{
		List<string> connectionIds;

		lock (_onlineUsers)
		{
			connectionIds = _onlineUsers.GetValueOrDefault(userId);
		}
		return Task.FromResult(connectionIds);
	}
}