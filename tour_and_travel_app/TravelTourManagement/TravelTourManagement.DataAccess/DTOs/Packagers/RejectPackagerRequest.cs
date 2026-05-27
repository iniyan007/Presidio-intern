using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public record RejectPackagerRequest(
    [Required]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Rejection reason must be between 10 and 500 characters.")]
    string Reason
);
