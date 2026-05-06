using backend.DTOs;
using backend.Models;

namespace backend.Interfaces
{
    public interface IOperatorService
    {
        Task<(bool Success, string Message, Bus? Bus)> AddBusAsync(int operatorId, CreateBusRequest request);
        Task<IEnumerable<Bus>> GetMyBusesAsync(int operatorId);

        Task<(bool Success, string Message, object? TripData)> CreateTripAsync(int operatorId, CreateTripRequest request);
        Task<IEnumerable<object>> GetMyTripsAsync(int operatorId);
        Task<(bool Success, string Message)> DeleteTripAsync(int operatorId, int tripId);
        Task<(bool Success, string Message, object? Data)> GetTripPassengersAsync(int operatorId, int tripId);

        Task<object> GetRevenueStatsAsync(int operatorId);
        Task<(bool Success, string Message)> AddExpenseAsync(int operatorId, CreateExpenseRequest request);
    }
}
