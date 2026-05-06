namespace backend.Interfaces
{
    public interface ITripService
    {
        Task<IEnumerable<object>> SearchTripsAsync(string? source, string? destination, DateTime? date);
        Task<object?> GetTripDetailsAsync(int id);
    }
}
