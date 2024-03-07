using System.Text.RegularExpressions;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
	Task<IEnumerable<MessageDTO>> CreateMessageThreadAsync(int senderId, int recipientId);
	Task<bool> CreateMessageAsync(Message message);
	Task<bool> DeleteMessageAsync(Message message);
	Task<IEnumerable<ContactDTO>> GetContactsWithMessageThreadAsync(int currentUserId);
	Task<bool> SaveAllAsync();
}
