using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public class CreateBookingCombinedRequest
{
    [Required]
    public string BookingData { get; set; } = null!;

    public List<IFormFile>? DocumentFiles { get; set; }
}
