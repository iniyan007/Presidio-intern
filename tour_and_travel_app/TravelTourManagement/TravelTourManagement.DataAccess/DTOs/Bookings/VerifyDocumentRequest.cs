using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public class VerifyDocumentRequest
{
    [Required]
    public bool IsVerified { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
