namespace API.SignalR;

public class PresenceTracker
{
	private static readonly Dictionary<int, List<string>> _onlineUsers = new();

	public Task UserConnected(int userId, string connectionId)
	{
		lock (_onlineUsers)
		{
			if (_onlineUsers.ContainsKey(userId))
			{
				_onlineUsers[userId].Add(connectionId);
			}
			else
			{
				_onlineUsers.Add(userId, new List<string> { connectionId });
			}
		}
		return Task.CompletedTask;
	}

	public Task UserDisconnected(int userId, string connectionId)
	{
		lock (_onlineUsers)
		{
			if (!_onlineUsers.ContainsKey(userId))
				return Task.CompletedTask;

			_onlineUsers[userId].Remove(connectionId);

			if (_onlineUsers[userId].Count == 0)
				_onlineUsers.Remove(userId);
		}

		return Task.CompletedTask;
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