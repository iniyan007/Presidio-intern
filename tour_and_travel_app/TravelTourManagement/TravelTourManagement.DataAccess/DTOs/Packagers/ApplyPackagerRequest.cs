using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public class ApplyPackagerRequest
{
    [Required]
    public string CompanyName { get; set; } = null!;
    public string? BusinessLicenseNo { get; set; }
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    
    [Required]
    public IFormFile PanDocument { get; set; } = null!;
    
    [Required]
    public IFormFile GstDocument { get; set; } = null!;
    
    [Required]
    public IFormFile BusinessRegistration { get; set; } = null!;
}
