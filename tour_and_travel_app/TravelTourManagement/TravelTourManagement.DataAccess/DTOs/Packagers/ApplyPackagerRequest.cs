namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public record ApplyPackagerRequest(
    string CompanyName,
    string? BusinessLicenseNo,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? WebsiteUrl
);
