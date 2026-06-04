using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Traveler")] // Usually only travelers have wishlists
public class WishlistsController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistsController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    /// <summary>
    /// Gets all wishlisted packages for the current user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyWishlists(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var wishlists = await _wishlistService.GetUserWishlistsAsync(userId, cancellationToken);
        return Ok(new { success = true, data = wishlists });
    }

    /// <summary>
    /// Toggles a package in the wishlist (adds if missing, removes if present).
    /// </summary>
    [HttpPost("toggle/{packageId:guid}")]
    public async Task<IActionResult> ToggleWishlist(Guid packageId, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var wasAdded = await _wishlistService.ToggleWishlistAsync(userId, packageId, cancellationToken);
        
        var message = wasAdded 
            ? "Package added to wishlist successfully." 
            : "Package removed from wishlist successfully.";

        return Ok(new { success = true, message, added = wasAdded });
    }
}
