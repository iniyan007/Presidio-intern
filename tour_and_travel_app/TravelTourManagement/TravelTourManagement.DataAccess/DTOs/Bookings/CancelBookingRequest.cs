using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public class CancelBookingRequest
{
    [Required]
    [MaxLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters.")]
    public string CancellationReason { get; set; } = null!;
}
