using System;

namespace TravelTourManagement.DataAccess.DTOs.PlatformConfig;

public class PlatformConfigResponse
{
    public Guid Id { get; set; }
    public decimal PlatformFeePercent { get; set; }
    public decimal GstPercent { get; set; }
    public string? Note { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
