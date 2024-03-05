using System.Numerics;
using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IGroupMessageRepository
{
	Task<IEnumerable<GroupMessageDTO>> CreateGroupMessageChannelAsync(string channelName, int senderId,
		IEnumerable<int> contactIds);
	Task<bool> CreateGroupMessageAsync(GroupMessage message);
	Task<IEnumerable<GroupMessageDTO>> GetGroupMessageChannelAsync(Guid channelId);
	Task<IEnumerable<GroupMessageDTO>> GetGroupMessageChannelNamesAsync(int userId);
	Task<IEnumerable<ContactDTO>> GetGroupMessageChannelContactsAsync(Guid channelId);
	Task<bool> DeleteGroupMessageAsync(GroupMessage message);
	Task<bool> SaveAllAsync();
}