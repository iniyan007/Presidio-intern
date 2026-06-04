using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Wishlists;

namespace TravelTourManagement.Business.Interface;

public interface IWishlistService
{
    /// <summary>
    /// Toggles a package in the user's wishlist. If it exists, removes it. If it doesn't, adds it.
    /// Returns true if added, false if removed.
    /// </summary>
    Task<bool> ToggleWishlistAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all wishlisted packages for a user.
    /// </summary>
    Task<IEnumerable<WishlistResponse>> GetUserWishlistsAsync(Guid userId, CancellationToken cancellationToken = default);
}
