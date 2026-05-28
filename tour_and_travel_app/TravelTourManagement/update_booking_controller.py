file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.API/Controllers/BookingsController.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

old_create = '''    [HttpPost]
    [Authorize(Roles = "Admin,Traveler")]
    public async Task<IActionResult> CreateBooking([FromForm] CreateBookingRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _bookingService.CreateBookingAsync(userId, request);
        return Ok(response);
    }'''

new_create = '''    [HttpPost]
    [Authorize(Roles = "Admin,Traveler")]
    public async Task<IActionResult> CreateBooking([FromForm] CreateBookingCombinedRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        CreateBookingRequest bookingData;
        try
        {
            bookingData = System.Text.Json.JsonSerializer.Deserialize<CreateBookingRequest>(request.BookingData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            
            // Manual validation
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(bookingData, serviceProvider: null, items: null);
            var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(bookingData, context, results, true);
            if (!isValid)
            {
                return BadRequest(results);
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { message = "Invalid JSON in BookingData.", details = ex.Message });
        }

        var response = await _bookingService.CreateBookingAsync(userId, bookingData, request.DocumentFiles);
        return Ok(response);
    }'''

content = content.replace(old_create, new_create)

with open(file_path, 'w') as f:
    f.write(content)
