using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public class CreatePackageCombinedRequest
{
    [Required]
    public string PackageData { get; set; } = null!;

    public List<IFormFile>? MediaFiles { get; set; }
}
