using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.DataAccess.DTOs.Wishlists;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Services;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public WishlistService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<bool> ToggleWishlistAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default)
    {
        // Check if package exists and is published
        var packageExists = await _context.Packages
            .AnyAsync(p => p.Id == packageId && p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published, cancellationToken);
            
        if (!packageExists)
            throw new KeyNotFoundException("Package not found or not published.");

        var existingWishlist = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.PackageId == packageId, cancellationToken);

        if (existingWishlist != null)
        {
            // Remove from wishlist
            _context.Wishlists.Remove(existingWishlist);
            await _context.SaveChangesAsync(cancellationToken);
            return false; // Indicating it was removed
        }
        else
        {
            // Add to wishlist
            var newWishlist = new Wishlist
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = packageId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Wishlists.Add(newWishlist);
            await _context.SaveChangesAsync(cancellationToken);
            return true; // Indicating it was added
        }
    }

    public async Task<IEnumerable<WishlistResponse>> GetUserWishlistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wishlists = await _context.Wishlists
            .Include(w => w.Package)
                .ThenInclude(p => p.Packager)
            .Include(w => w.Package)
                .ThenInclude(p => p.PackageMedia)
            .Include(w => w.Package)
                .ThenInclude(p => p.PackageSeasonalPricings)
            .Where(w => w.UserId == userId && w.Package.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        var responses = wishlists.Select(w => new WishlistResponse(
            w.Id,
            w.PackageId,
            w.CreatedAt,
            _mapper.Map<PackageSummaryResponse>(w.Package)
        )).ToList();

        return responses;
    }
}
