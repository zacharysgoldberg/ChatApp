using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessageAsync(Message message);
    void DeleteMessageAsync(Message message);
    Task<Message> GetMessageAsync(int id);
    Task<PagedList<MessageDTO>> GetMessagesAsync(MessageParams messageParams);
    Task<IEnumerable<MessageDTO>> GetMessageThreadAsync(int currentUserId, int recipientId);
    Task<IEnumerable<ContactDTO>> GetContactsWithMessageThreadAsync(int currentUserId);
    Task<bool> SaveAllAsync();
}
