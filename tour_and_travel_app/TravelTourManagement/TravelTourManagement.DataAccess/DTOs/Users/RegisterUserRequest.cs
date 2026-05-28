using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Users;

public class RegisterUserRequest
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Indian phone number. It must be 10 digits and start with 6-9.")]
    public string? Phone { get; set; }
}
