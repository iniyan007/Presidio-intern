using TravelTourManagement.DataAccess.DTOs.Messages;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Business.Interface;

public interface IMessageService
{
    Task<MessageThreadDto> GetOrInitializeThreadAsync(Guid userId, Guid packagerId, Guid? packageId, CancellationToken cancellationToken = default);
    Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageDto>> GetThreadMessagesAsync(Guid threadId, Guid requestorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageThreadDto>> GetUserThreadsAsync(Guid userId, bool isPackager, CancellationToken cancellationToken = default);
    Task<bool> MarkMessagesAsReadAsync(Guid threadId, Guid requestorId, MessageSenderRole readerRole, CancellationToken cancellationToken = default);
}
