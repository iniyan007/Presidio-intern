using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Users;

public class SendOtpRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;
}
