using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.DataAccess.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    // Define tables that produce too much noise to be audited
    private readonly HashSet<string> _excludedEntities = new()
    {
        nameof(AuditLog),
        nameof(Message),
        nameof(Notification),
        "JwtToken" // If applicable
    };

    public AuditInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = new List<AuditLog>();

        // We resolve IUserContextService here using CreateScope to avoid capturing scoped services in singleton options (if DbContext options are singleton)
        // DbContext is scoped, so we could technically inject IUserContextService into the Interceptor if registered as scoped, but interceptor instances are often shared.
        // It's safer to resolve it from the Context's own service provider.
        IUserContextService? userContextService = null;
        try 
        {
            userContextService = eventData.Context.GetService<IUserContextService>();
        }
        catch 
        {
            // Fallback to the root provider if available (e.g. testing)
            using var scope = _serviceProvider.CreateScope();
            userContextService = scope.ServiceProvider.GetService<IUserContextService>();
        }

        var userId = userContextService?.UserId;
        var ipAddress = userContextService?.IpAddress;
        var userAgent = userContextService?.UserAgent;

        var entries = eventData.Context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var entityName = entry.Metadata.Name.Split('.').Last(); // Get simple class name

            if (_excludedEntities.Contains(entityName))
                continue;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityName,
                Action = entry.State.ToString(),
                PerformedBy = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                PerformedAt = DateTime.UtcNow
            };

            // Try to extract primary key (assuming Guid Id)
            var primaryKeyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (primaryKeyProperty != null && primaryKeyProperty.CurrentValue is Guid entityId)
            {
                auditLog.EntityId = entityId;
                
                // Fallback for Login/Register where UserContext is null but we are modifying a User
                if (auditLog.PerformedBy == null && entityName == "User")
                {
                    auditLog.PerformedBy = entityId;
                }
            }
            else
            {
                // If it's a composite key or non-guid key, we can't easily map it to EntityId Guid column. We just generate a random one or skip.
                // In this schema, EntityId is a required Guid. Most entities use Guid.
                if (primaryKeyProperty?.CurrentValue != null)
                {
                    if (Guid.TryParse(primaryKeyProperty.CurrentValue.ToString(), out var parsedId))
                    {
                        auditLog.EntityId = parsedId;
                    }
                    else
                    {
                        continue; // Skip entities without Guid keys for audit logging
                    }
                }
                else
                {
                    // If Added state and Id is DB generated, it won't have it yet.
                    // EF Core usually generates UUID client side, but just in case:
                    auditLog.EntityId = Guid.Empty; 
                }
            }

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue; // Ignore temporary keys generated by EF

                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            if (oldValues.Any())
                auditLog.OldValues = JsonSerializer.Serialize(oldValues);

            if (newValues.Any())
                auditLog.NewValues = JsonSerializer.Serialize(newValues);

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Any())
        {
            eventData.Context.Set<AuditLog>().AddRange(auditEntries);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
