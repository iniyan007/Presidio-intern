using TravelTourManagement.DataAccess.DTOs.Messages;

namespace TravelTourManagement.Business.Interface;

public interface IMessageDispatcher
{
    Task DispatchMessageAsync(Guid travelerUserId, Guid packagerUserId, MessageDto message, CancellationToken cancellationToken = default);
}
