using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public class UploadPackageMediaRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    
    [Required]
    public string Category { get; set; } = null!;
    
    [Required]
    public bool IsPrimary { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public string? Caption { get; set; }
}
