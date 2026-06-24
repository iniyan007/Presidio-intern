using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TravelTourManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly IPackageRepository _packageRepository;

    public MetadataController(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

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

    /// <summary>
    /// Retrieves a combined list of predefined standard countries and any dynamically added countries from the database.
    /// </summary>
    [HttpGet("countries")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCountries(System.Threading.CancellationToken cancellationToken)
    {
        var standardCountries = new List<string> {
    "India",
    "United States",
    "United Kingdom",
    "Australia",
    "Canada",
    "Singapore",
    "Malaysia",
    "United Arab Emirates",
    "Thailand",
    "Maldives",
    "Sri Lanka",
    "France",
    "Germany",
    "Italy",
    "Switzerland",
    "Japan",
    "China",
    "South Korea",
    "Indonesia",
    "Vietnam",
    "Philippines",
    "Nepal",
    "Bhutan",
    "Bangladesh",
    "Pakistan",
    "New Zealand",
    "Turkey",
    "Saudi Arabia",
    "Qatar",
    "Oman",
    "Kuwait",
    "Bahrain",
    "Egypt",
    "South Africa",
    "Kenya",
    "Mauritius",
    "Russia",
    "Ukraine",
    "Netherlands",
    "Belgium",
    "Austria",
    "Spain",
    "Portugal",
    "Greece",
    "Norway",
    "Sweden",
    "Denmark",
    "Finland",
    "Ireland",
    "Poland",
    "Czech Republic",
    "Hungary",
    "Romania",
    "Croatia",
    "Serbia",
    "Bulgaria",
    "Mexico",
    "Brazil",
    "Argentina",
    "Chile",
    "Peru",
    "Colombia"
};
        var dynamicCountries = await _packageRepository.GetDistinctCountriesAsync(cancellationToken);
        
        var combined = standardCountries.Union(dynamicCountries).OrderBy(c => c).ToList();
        return Ok(combined);
    }

    /// <summary>
    /// Retrieves a combined list of predefined standard destinations and any dynamically added destinations from the database.
    /// Supports an optional 'country' query parameter to filter destinations by a specific country.
    /// </summary>
    [HttpGet("destinations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDestinations([FromQuery] string? country, System.Threading.CancellationToken cancellationToken)
    {
        var standardDestinationsMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Kerala", "India" }, { "Goa", "India" }, { "Delhi", "India" },
            { "Bali", "Indonesia" }, { "Paris", "France" }, { "London", "UK" },
            { "New York", "USA" }, { "Dubai", "UAE" }, { "Sydney", "Australia" },
            { "Rome", "Italy" }, { "Tokyo", "Japan" }, { "Male", "Maldives" },
            { "Colombo", "Sri Lanka" }
        };

        IEnumerable<string> standardDestinations = standardDestinationsMap.Keys;

        if (!string.IsNullOrWhiteSpace(country))
        {
            standardDestinations = standardDestinationsMap
                .Where(kvp => string.Equals(kvp.Value, country, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key);
        }

        var dynamicDestinations = await _packageRepository.GetDistinctDestinationsAsync(country, cancellationToken);
        
        var combined = standardDestinations.Union(dynamicDestinations).OrderBy(d => d).ToList();
        return Ok(combined);
    }
}
