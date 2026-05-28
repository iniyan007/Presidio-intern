file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.DataAccess/DTOs/Packages/CreatePackageRequest.cs'
with open(file_path, 'r') as f:
    content = f.read()

# Update CreatePackagePricingRequest
old_pricing = '''public record CreatePackagePricingRequest(
    [Required] string SeasonName,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required] [Range(0.01, 10000000, ErrorMessage = "BasePrice must be strictly positive.")] decimal BasePrice,
    [Range(0, 10000000, ErrorMessage = "ChildPrice must be non-negative.")] decimal ChildPrice,
    [Range(0, 100, ErrorMessage = "DiscountPercent must be between 0 and 100")] decimal DiscountPercent,
    [Required] [Range(1, 10000, ErrorMessage = "AvailableSlots must be strictly positive.")] int AvailableSlots,
    bool IsActive
);'''

new_pricing = '''public record CreatePackagePricingRequest(
    [Required] string SeasonName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    [Required] [Range(0.01, 10000000, ErrorMessage = "BasePrice must be strictly positive.")] decimal BasePrice,
    [Range(0, 10000000, ErrorMessage = "ChildPrice must be non-negative.")] decimal ChildPrice,
    [Range(0, 100, ErrorMessage = "DiscountPercent must be between 0 and 100")] decimal DiscountPercent,
    [Required] [Range(1, 10000, ErrorMessage = "AvailableSlots must be strictly positive.")] int AvailableSlots,
    bool IsActive
);'''

content = content.replace(old_pricing, new_pricing)

# Update validation logic
old_validation = '''        if (!string.Equals(Country, "India", StringComparison.OrdinalIgnoreCase))
        {
            if (SeasonalPricing != null && SeasonalPricing.Any())
            {
                var earliestStartDate = SeasonalPricing.Min(p => p.StartDate);
                var tenMonthsFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(10));
                
                if (earliestStartDate < tenMonthsFromNow)
                {
                    results.Add(new ValidationResult("For international packages, the start date (earliest seasonal pricing) must be at least 10 months ahead of the current date.", new[] { nameof(SeasonalPricing) }));
                }
            }'''

new_validation = '''        // Honeymoon Package Rules
        bool isHoneymoon = string.Equals(Type, "Honeymoon", StringComparison.OrdinalIgnoreCase);

        if (SeasonalPricing != null)
        {
            foreach (var pricing in SeasonalPricing)
            {
                if (!isHoneymoon && (!pricing.StartDate.HasValue || !pricing.EndDate.HasValue))
                {
                    results.Add(new ValidationResult("StartDate and EndDate are required for non-Honeymoon packages.", new[] { nameof(SeasonalPricing) }));
                }
            }
        }

        if (!string.Equals(Country, "India", StringComparison.OrdinalIgnoreCase))
        {
            if (SeasonalPricing != null && SeasonalPricing.Any())
            {
                // Only validate if StartDate is provided (e.g. for non-Honeymoon, or Honeymoon with explicit dates)
                var validStartDates = SeasonalPricing.Where(p => p.StartDate.HasValue).Select(p => p.StartDate!.Value).ToList();
                if (validStartDates.Any())
                {
                    var earliestStartDate = validStartDates.Min();
                    var tenMonthsFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(10));
                    
                    if (earliestStartDate < tenMonthsFromNow)
                    {
                        results.Add(new ValidationResult("For international packages, the start date (earliest seasonal pricing) must be at least 10 months ahead of the current date.", new[] { nameof(SeasonalPricing) }));
                    }
                }
            }'''

content = content.replace(old_validation, new_validation)

with open(file_path, 'w') as f:
    f.write(content)
