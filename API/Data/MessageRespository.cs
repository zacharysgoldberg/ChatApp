using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API;

public class MessageRespository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public MessageRespository(DataContext context, IUserRepository userRepository, IMapper mapper)
    {
        _context        = context;
        _userRepository = userRepository;
        _mapper         = mapper;
    }

    public void AddMessageAsync(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessageAsync(Message message)
    {
        _context.Remove(message);
    }

    public async Task<Message> GetMessageAsync(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDTO>> GetMessagesAsync(MessageParams messageParams)
    {
        IQueryable<Message> query = _context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(u => u.RecipientId == messageParams.Id),
            "Outbox" => query.Where(u => u.SenderId == messageParams.Id),
            _ => query.Where(u => u.RecipientId == messageParams.Id)
        };

        IQueryable<MessageDTO> messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDTO>
            .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDTO>> GetMessageThreadAsync(int currentUserId, 
        int recipientId)
    {
        ICollection<Message> messages = await _context.Messages
            .Include(m => m.Sender)
            .ThenInclude(m => m.Photo)
            .Where
            (
                m => m.RecipientId == currentUserId 
                && m.SenderId == recipientId ||
                m.RecipientId == recipientId &&
                m.SenderId == currentUserId
            )
            .OrderBy(m => m.MessageSent)
            .ToListAsync();

        ICollection<Message> unreadMessages = messages
            .Where(m => m.RecipientId == currentUserId)
            .ToList();

        List<MessageDTO> messageDTOs = new();

         foreach (Message message in messages)
        {
            MemberDTO sender             = await _userRepository.GetMemberByIdAsync(message.SenderId);
            MemberDTO recipient          = await _userRepository.GetMemberByIdAsync(message.RecipientId);

            MessageDTO messageDTO        = _mapper.Map<MessageDTO>(message);
            messageDTO.SenderUsername    = sender.UserName;
            messageDTO.RecipientUsername = recipient.UserName;

            messageDTOs.Add(messageDTO);
        }

        return messageDTOs;
    }

    public async Task<IEnumerable<ContactDTO>> GetContactsWithMessageThreadAsync(int currentUserId)
    {
        List<int> usersIds = await _context.Messages
            .Where(m => m.RecipientId == currentUserId || m.SenderId == currentUserId)
            .Select(m => m.RecipientId == currentUserId ? m.SenderId : m.RecipientId)
            .Distinct()
            .ToListAsync();

        if (usersIds == null)
            return null;

        List<ContactDTO> contactDTOs = new();

        foreach (int userId in usersIds)
        {
            MemberDTO memberDTO = await _userRepository.GetMemberByIdAsync(userId);
            contactDTOs.Add(_mapper.Map<ContactDTO>(memberDTO));
        }
        return contactDTOs;
    }


    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
