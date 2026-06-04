using System;
using TravelTourManagement.DataAccess.DTOs.Packages;

namespace TravelTourManagement.DataAccess.DTOs.Wishlists;

public record WishlistResponse(
    Guid WishlistId,
    Guid PackageId,
    DateTime AddedAt,
    PackageSummaryResponse Package
);
