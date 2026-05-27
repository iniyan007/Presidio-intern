using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Users;

public record VerifyOtpRequest(
    [Required]
    [StringLength(6, MinimumLength = 6)]
    string Otp
);
