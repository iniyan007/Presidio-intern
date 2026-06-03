using System;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public class UpdatePackageDetailsRequest
{
    [Required] [MaxLength(200)] public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    [Required] [MaxLength(200)] public string Destination { get; set; } = null!;
    [Required] [MaxLength(100)] public string Country { get; set; } = null!;
    [MaxLength(100)] public string? City { get; set; }

    [Required] [Range(1, 365, ErrorMessage = "DurationDays must be between 1 and 365")] 
    public int DurationDays { get; set; }
    
    [Range(0, 365, ErrorMessage = "DurationNights must be between 0 and 365")] 
    public int DurationNights { get; set; }

    [Required] [Range(1, 1000, ErrorMessage = "MaxCapacity must be between 1 and 1000")] 
    public int MaxCapacity { get; set; }

    [Range(0, 120, ErrorMessage = "MinAge must be between 0 and 120")] 
    public int? MinAge { get; set; }

    public string? CancellationPolicy { get; set; }
}
