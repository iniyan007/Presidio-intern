file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/BookingService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

old_booking = '''        if (package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Honeymoon)
        {
            var oneMonthFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
            if (request.TravelDate < oneMonthFromNow)
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException("Honeymoon packages must be booked at least 1 month in advance.");
            }
        }'''

new_booking = '''        if (package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Honeymoon ||
            package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Private ||
            package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Family)
        {
            var oneMonthFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
            if (request.TravelDate < oneMonthFromNow)
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException($"{package.Type} packages must be booked at least 1 month in advance.");
            }
        }'''

content = content.replace(old_booking, new_booking)

with open(file_path, 'w') as f:
    f.write(content)
