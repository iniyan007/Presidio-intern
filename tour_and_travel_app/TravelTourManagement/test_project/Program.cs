using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.Business.Mappings;
using System.Linq;

class Program {
    static void Main() {
        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(PackageMappingProfile));
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var package = new Package {
            Id = Guid.NewGuid(),
            PackagerId = Guid.NewGuid(),
            Title = "Test",
            Type = TravelTourManagement.DataAccess.Enums.PackageType.Honeymoon,
            Destination = "Dest",
            Country = "Country",
            DurationDays = 1,
            DurationNights = 1,
            MaxCapacity = 2,
            CurrentBookings = 0,
            ItineraryDays = new List<ItineraryDay> {
                new ItineraryDay {
                    Id = Guid.NewGuid(),
                    DayNumber = 1,
                    Title = "Day 1",
                    ItineraryActivities = null // this might be null
                }
            }
        };

        try {
            var result = mapper.Map<PackageDetailResponse>(package);
            Console.WriteLine("Success: " + result.Title);
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
