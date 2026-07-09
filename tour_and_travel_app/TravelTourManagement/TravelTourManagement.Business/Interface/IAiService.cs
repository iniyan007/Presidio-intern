using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Ai;

namespace TravelTourManagement.Business.Interface;

public interface IAiService
{
    Task<ChatResponseDto> GenerateChatResponseAsync(ChatRequestDto request, CancellationToken cancellationToken = default);
}
