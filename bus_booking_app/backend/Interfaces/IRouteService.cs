using backend.Models;

namespace backend.Interfaces
{
    public interface IRouteService
    {
        Task<IEnumerable<BusRoute>> GetRoutesAsync();
    }
}
