using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Messages;

public class CreateThreadRequest
{
    [Required]
    public Guid PackagerId { get; set; }
    
    public Guid? PackageId { get; set; }
}
