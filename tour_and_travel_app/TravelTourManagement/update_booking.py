import re

file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/BookingService.cs'
with open(file_path, 'r') as f:
    content = f.read()

old_logic = '''        var pricing = await _seasonalPricingRepository.GetByIdAsync(request.SeasonalPricingId, cancellationToken);
        if (pricing == null || pricing.PackageId != package.Id || !pricing.IsActive)
            throw new InvalidOperationException("Pricing tier is not valid or active.");'''

new_logic = '''        var pricing = await _seasonalPricingRepository.GetByIdAsync(request.SeasonalPricingId, cancellationToken);
        if (pricing == null || pricing.PackageId != package.Id || !pricing.IsActive)
            throw new InvalidOperationException("Pricing tier is not valid or active.");

        if (package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Honeymoon)
        {
            var oneMonthFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
            if (request.TravelDate < oneMonthFromNow)
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException("Honeymoon packages must be booked at least 1 month in advance.");
            }
        }'''

content = content.replace(old_logic, new_logic)

with open(file_path, 'w') as f:
    f.write(content)
