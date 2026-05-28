file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Interface/IBookingService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

if "using Microsoft.AspNetCore.Http;" not in content:
    content = content.replace("using TravelTourManagement.DataAccess.DTOs.Bookings;", "using TravelTourManagement.DataAccess.DTOs.Bookings;\nusing Microsoft.AspNetCore.Http;")

old_method = "Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, CancellationToken cancellationToken = default);"
new_method = "Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, List<IFormFile>? documentFiles = null, CancellationToken cancellationToken = default);"

content = content.replace(old_method, new_method)

with open(file_path, 'w') as f:
    f.write(content)
