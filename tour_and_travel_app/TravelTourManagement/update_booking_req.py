file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.DataAccess/DTOs/Bookings/CreateBookingRequest.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

old_files = '''    public IFormFile? AadharCardFile { get; set; }

    public IFormFile? PassportFile { get; set; }'''

new_files = '''    public string? AadharCardFileName { get; set; }

    public string? PassportFileName { get; set; }'''

content = content.replace(old_files, new_files)

# Remove using Microsoft.AspNetCore.Http; as it's no longer needed here but it won't hurt to leave it or remove it.

with open(file_path, 'w') as f:
    f.write(content)
