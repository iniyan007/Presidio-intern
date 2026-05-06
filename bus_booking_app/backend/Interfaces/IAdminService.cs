using backend.DTOs;
using backend.Models;

namespace backend.Interfaces
{
    public interface IAdminService
    {
        Task<object> GetStatsAsync();
        
        Task<IEnumerable<BusRoute>> GetRoutesAsync();
        Task<(bool Success, string Message, BusRoute? Route)> CreateRouteAsync(CreateRouteRequest request);
        Task<(bool Success, string Message)> DeleteRouteAsync(int id);
        
        Task<IEnumerable<object>> GetPendingOperatorsAsync();
        Task<IEnumerable<object>> GetAllOperatorsAsync();
        Task<(bool Success, string Message)> ApproveOperatorAsync(int id);
        Task<(bool Success, string Message)> RejectOperatorAsync(int id);

        Task<IEnumerable<object>> GetPendingBusesAsync();
        Task<IEnumerable<object>> GetAllBusesAsync();
        Task<(bool Success, string Message)> ApproveBusAsync(int id);
        Task<(bool Success, string Message)> RejectBusAsync(int id);
    }
}
