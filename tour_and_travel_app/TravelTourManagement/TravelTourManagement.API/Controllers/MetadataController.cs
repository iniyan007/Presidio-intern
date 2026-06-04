using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    /// <summary>
    /// Retrieves all the predefined Enums for frontend dropdown population.
    /// </summary>
    [HttpGet("enums")]
    [AllowAnonymous]
    public IActionResult GetEnums()
    {
        return Ok(new
        {
            PackageTypes = Enum.GetNames(typeof(PackageType)),
            PackageStatuses = Enum.GetNames(typeof(PackageStatus)),
            InclusionTypes = Enum.GetNames(typeof(InclusionType)),
            MediaCategories = Enum.GetNames(typeof(MediaCategory)),
            MealTypes = Enum.GetNames(typeof(MealType)),
            TransportModes = Enum.GetNames(typeof(TransportMode)),
            DaySessions = Enum.GetNames(typeof(DaySession)),
            BookingStatuses = Enum.GetNames(typeof(BookingStatus)),
            PaymentStatuses = Enum.GetNames(typeof(PaymentStatus))
        });
    }
}
