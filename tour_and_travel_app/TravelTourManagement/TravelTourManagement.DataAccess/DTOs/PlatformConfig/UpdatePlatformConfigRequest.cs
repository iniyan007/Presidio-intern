using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.PlatformConfig;

public class UpdatePlatformConfigRequest
{
    [Required]
    [Range(0, 100)]
    public decimal PlatformFeePercent { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal GstPercent { get; set; }

    public string? Note { get; set; }
}
