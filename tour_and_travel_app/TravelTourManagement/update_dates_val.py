file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.DataAccess/DTOs/Packages/CreatePackageRequest.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

old_val = '''        // Honeymoon Package Rules
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
        }'''

new_val = '''        // Flexible Dates Package Rules
        bool isFlexibleDatePackage = string.Equals(Type, "Honeymoon", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(Type, "Private", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(Type, "Family", StringComparison.OrdinalIgnoreCase);

        if (SeasonalPricing != null)
        {
            foreach (var pricing in SeasonalPricing)
            {
                if (!isFlexibleDatePackage && (!pricing.StartDate.HasValue || !pricing.EndDate.HasValue))
                {
                    results.Add(new ValidationResult("StartDate and EndDate are required unless the package type is Honeymoon, Private, or Family.", new[] { nameof(SeasonalPricing) }));
                }
            }
        }'''

content = content.replace(old_val, new_val)

with open(file_path, 'w') as f:
    f.write(content)
