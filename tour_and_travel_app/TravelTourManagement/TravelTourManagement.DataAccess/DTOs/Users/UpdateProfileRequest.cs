using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Users;

public record UpdateProfileRequest(
    [Required]
    [StringLength(150, MinimumLength = 2)]
    string FullName,
    
    [StringLength(20)]
    string? Phone
);
