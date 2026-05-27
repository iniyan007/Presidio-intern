using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.DataAccess.Repository;

/// <summary>
/// EF Core repository for the <see cref="ItineraryDayMeal"/> entity.
/// </summary>
public class ItineraryDayMealRepository : GenericRepository<ItineraryDayMeal, Guid>, IItineraryDayMealRepository
{
    public ItineraryDayMealRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ItineraryDayMeal>> GetByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.ItineraryDayId == itineraryDayId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task DeleteByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
    {
        var meals = await _dbSet
            .Where(m => m.ItineraryDayId == itineraryDayId)
            .ToListAsync(cancellationToken);

        if (meals.Count > 0)
        {
            _dbSet.RemoveRange(meals);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
