namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public class PackagerSearchRequest
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
