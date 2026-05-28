import re

file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/PackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

old_pricing = '''            PackageSeasonalPricings = request.SeasonalPricing?.Select(p => new PackageSeasonalPricing
            {
                SeasonName = p.SeasonName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,'''

new_pricing = '''            PackageSeasonalPricings = request.SeasonalPricing?.Select(p => new PackageSeasonalPricing
            {
                SeasonName = p.SeasonName,
                StartDate = p.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = p.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)),
                BasePrice = p.BasePrice,'''

content = content.replace(old_pricing, new_pricing)

with open(file_path, 'w') as f:
    f.write(content)
