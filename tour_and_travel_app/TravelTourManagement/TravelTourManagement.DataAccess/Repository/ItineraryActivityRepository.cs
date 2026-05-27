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
/// EF Core repository for the <see cref="ItineraryActivity"/> entity.
/// </summary>
public class ItineraryActivityRepository : GenericRepository<ItineraryActivity, Guid>, IItineraryActivityRepository
{
    public ItineraryActivityRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ItineraryActivity>> GetByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(a => a.ItineraryDayId == itineraryDayId)
            .OrderBy(a => a.SequenceOrder)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task ReorderAsync(
        IEnumerable<Guid> orderedActivityIds,
        CancellationToken cancellationToken = default)
    {
        var orderDict = orderedActivityIds
            .Select((id, index) => new { Id = id, SequenceOrder = index + 1 })
            .ToDictionary(x => x.Id, x => x.SequenceOrder);

        if (orderDict.Count == 0) return;

        var ids = orderDict.Keys.ToList();
        var activitiesToUpdate = await _dbSet
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);

        foreach (var activity in activitiesToUpdate)
        {
            if (orderDict.TryGetValue(activity.Id, out int newOrder))
            {
                activity.SequenceOrder = newOrder;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
