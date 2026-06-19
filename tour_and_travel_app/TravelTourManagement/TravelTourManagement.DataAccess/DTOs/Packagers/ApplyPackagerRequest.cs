using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public class ApplyPackagerRequest
{
    [Required]
    public string CompanyName { get; set; } = null!;
    
    [Required]
    public string? BusinessLicenseNo { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    
    [Required]
    public string ContactEmail { get; set; } = null!;
    
    [Required]
    public string ContactPhone { get; set; } = null!;
    
    [Required]
    public string WebsiteUrl { get; set; } = null!;
    
    [Required]
    public IFormFile PanDocument { get; set; } = null!;
    
    [Required]
    public IFormFile GstDocument { get; set; } = null!;
    
    [Required]
    public IFormFile BusinessRegistration { get; set; } = null!;
}
