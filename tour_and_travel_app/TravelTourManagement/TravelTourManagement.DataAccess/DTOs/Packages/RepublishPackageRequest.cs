using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public class RepublishPackageRequest : IValidatableObject
{
    [Required]
    public List<CreatePackagePricingRequest> SeasonalPricing { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        if (SeasonalPricing == null || !SeasonalPricing.Any())
        {
            results.Add(new ValidationResult("At least one seasonal pricing is required to republish.", new[] { nameof(SeasonalPricing) }));
        }

        return results;
    }
}
