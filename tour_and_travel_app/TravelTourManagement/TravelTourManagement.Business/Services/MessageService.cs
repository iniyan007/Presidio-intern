using Microsoft.EntityFrameworkCore;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.DTOs.Messages;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Business.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly IMessageDispatcher _dispatcher;

    public MessageService(ApplicationDbContext context, IMessageDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public async Task<MessageThreadDto> GetOrInitializeThreadAsync(Guid userId, Guid packagerId, Guid? packageId, CancellationToken cancellationToken = default)
    {
        var thread = await _context.MessageThreads
            .Include(t => t.User)
            .Include(t => t.Packager)
            .ThenInclude(p => p.User)
            .Include(t => t.Package)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.PackagerId == packagerId && t.PackageId == packageId, cancellationToken);

        if (thread == null)
        {
            // Verify packager exists
            var packagerExists = await _context.Packagers.AnyAsync(p => p.Id == packagerId, cancellationToken);
            if (!packagerExists) throw new KeyNotFoundException("Packager not found.");

            thread = new MessageThread
            {
                UserId = userId,
                PackagerId = packagerId,
                PackageId = packageId,
                CreatedAt = DateTime.UtcNow
            };

            _context.MessageThreads.Add(thread);
            await _context.SaveChangesAsync(cancellationToken);

            // Fetch fully loaded to map easily
            thread = await _context.MessageThreads
                .Include(t => t.User)
                .Include(t => t.Packager)
                .ThenInclude(p => p.User)
                .Include(t => t.Package)
                .FirstAsync(t => t.Id == thread.Id, cancellationToken);
        }

        return MapThreadToDto(thread);
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var thread = await _context.MessageThreads
            .Include(t => t.Packager)
            .FirstOrDefaultAsync(t => t.Id == request.ThreadId, cancellationToken);

        if (thread == null) throw new KeyNotFoundException("Chat thread not found.");

        // Validate sender is part of the thread
        if (request.SenderRole == MessageSenderRole.user && thread.UserId != senderId)
            throw new UnauthorizedAccessException("You are not part of this chat thread.");

        if (request.SenderRole == MessageSenderRole.packager)
        {
            var packager = await _context.Packagers.FirstOrDefaultAsync(p => p.UserId == senderId, cancellationToken);
            if (packager == null || thread.PackagerId != packager.Id)
                throw new UnauthorizedAccessException("You are not part of this chat thread.");
        }

        var message = new Message
        {
            ThreadId = thread.Id,
            SenderId = senderId,
            SenderRole = request.SenderRole,
            Body = request.Body,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        
        thread.LastMessageAt = message.SentAt;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MessageDto
        {
            Id = message.Id,
            ThreadId = message.ThreadId,
            SenderId = message.SenderId,
            SenderRole = message.SenderRole,
            Body = message.Body,
            IsRead = message.IsRead,
            SentAt = message.SentAt
        };

        // Notify via SignalR directly to both users
        await _dispatcher.DispatchMessageAsync(thread.UserId, thread.Packager.UserId, dto, cancellationToken);

        return dto;
    }

    public async Task<IEnumerable<MessageDto>> GetThreadMessagesAsync(Guid threadId, Guid requestorId, CancellationToken cancellationToken = default)
    {
        var thread = await _context.MessageThreads
            .Include(t => t.Packager)
            .FirstOrDefaultAsync(t => t.Id == threadId, cancellationToken);

        if (thread == null) throw new KeyNotFoundException("Chat thread not found.");

        if (thread.UserId != requestorId && thread.Packager.UserId != requestorId)
            throw new UnauthorizedAccessException("You are not part of this chat thread.");

        var messages = await _context.Messages
            .Where(m => m.ThreadId == threadId)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ThreadId = m.ThreadId,
                SenderId = m.SenderId,
                SenderRole = m.SenderRole,
                Body = m.Body,
                IsRead = m.IsRead,
                SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc)
            })
            .ToListAsync(cancellationToken);

        return messages;
    }

    public async Task<IEnumerable<MessageThreadDto>> GetUserThreadsAsync(Guid userId, bool isPackager, CancellationToken cancellationToken = default)
    {
        IQueryable<MessageThread> query = _context.MessageThreads
            .Include(t => t.User)
            .Include(t => t.Packager)
            .ThenInclude(p => p.User)
            .Include(t => t.Package);

        if (isPackager)
        {
            var packager = await _context.Packagers.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
            if (packager == null) return new List<MessageThreadDto>();
            query = query.Where(t => t.PackagerId == packager.Id);
        }
        else
        {
            query = query.Where(t => t.UserId == userId);
        }

        var threads = await query
            .OrderByDescending(t => t.LastMessageAt ?? t.CreatedAt)
            .ToListAsync(cancellationToken);

        var threadDtos = new List<MessageThreadDto>();
        foreach (var thread in threads)
        {
            var dto = MapThreadToDto(thread);
            
            var lastMessage = await _context.Messages
                .Where(m => m.ThreadId == thread.Id)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastMessage != null)
            {
                dto.LastMessage = new MessageDto
                {
                    Id = lastMessage.Id,
                    ThreadId = lastMessage.ThreadId,
                    SenderId = lastMessage.SenderId,
                    SenderRole = lastMessage.SenderRole,
                    Body = lastMessage.Body,
                    IsRead = lastMessage.IsRead,
                    SentAt = DateTime.SpecifyKind(lastMessage.SentAt, DateTimeKind.Utc)
                };
            }

            var readerRoleToCount = isPackager ? MessageSenderRole.user : MessageSenderRole.packager;
            dto.UnreadCount = await _context.Messages
                .CountAsync(m => m.ThreadId == thread.Id && !m.IsRead && m.SenderRole == readerRoleToCount, cancellationToken);

            threadDtos.Add(dto);
        }

        return threadDtos;
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid threadId, Guid requestorId, MessageSenderRole readerRole, CancellationToken cancellationToken = default)
    {
        var thread = await _context.MessageThreads
            .Include(t => t.Packager)
            .FirstOrDefaultAsync(t => t.Id == threadId, cancellationToken);

        if (thread == null) return false;

        if (readerRole == MessageSenderRole.user && thread.UserId != requestorId) return false;
        if (readerRole == MessageSenderRole.packager && thread.Packager.UserId != requestorId) return false;

        var senderRoleToMark = readerRole == MessageSenderRole.user ? MessageSenderRole.packager : MessageSenderRole.user;

        var unreadMessages = await _context.Messages
            .Where(m => m.ThreadId == threadId && !m.IsRead && m.SenderRole == senderRoleToMark)
            .ToListAsync(cancellationToken);

        if (!unreadMessages.Any()) return true;

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static MessageThreadDto MapThreadToDto(MessageThread thread)
    {
        return new MessageThreadDto
        {
            Id = thread.Id,
            UserId = thread.UserId,
            PackagerId = thread.PackagerId,
            PackageId = thread.PackageId,
            UserName = thread.User.FullName,
            UserProfilePicture = thread.User.ProfilePicture ?? string.Empty,
            PackagerName = thread.Packager.CompanyName,
            PackagerProfilePicture = thread.Packager.User.ProfilePicture ?? string.Empty,
            PackageTitle = thread.Package?.Title,
            CreatedAt = DateTime.SpecifyKind(thread.CreatedAt, DateTimeKind.Utc),
            LastMessageAt = thread.LastMessageAt.HasValue ? DateTime.SpecifyKind(thread.LastMessageAt.Value, DateTimeKind.Utc) : null
        };
    }
}
