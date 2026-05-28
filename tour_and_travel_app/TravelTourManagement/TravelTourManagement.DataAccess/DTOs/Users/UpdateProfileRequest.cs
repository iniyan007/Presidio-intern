using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Users;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Indian phone number. It must be 10 digits and start with 6-9.")]
    public string? Phone { get; set; }
}
