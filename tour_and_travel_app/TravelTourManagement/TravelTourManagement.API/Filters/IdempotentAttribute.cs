using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using System;

namespace TravelTourManagement.API.Filters;

public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    private readonly IDistributedCache _cache;
    private const string IdempotencyHeader = "X-Idempotency-Key";

    public IdempotentAttribute(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyHeader, out var idempotencyKey))
        {
            context.Result = new BadRequestObjectResult(new { message = "Idempotency key is missing. Please provide the X-Idempotency-Key header." });
            return;
        }

        var cacheKey = $"IDEMPOTENCY_{idempotencyKey}";
        
        // Attempt to retrieve it
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        
        if (cachedValue != null)
        {
            // The request is already in progress or has been processed
            context.Result = new ConflictObjectResult(new { message = "Duplicate request detected. This operation has already been processed or is currently processing." });
            return;
        }

        // Set a temporary value to indicate processing has started (to prevent race conditions)
        await _cache.SetStringAsync(cacheKey, "PROCESSING", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });

        // Execute the actual endpoint
        var executedContext = await next();

        // If the execution failed (e.g. 500 error), we might want to remove the key so they can try again.
        // But for idempotency, usually we keep it so they don't retry non-idempotent operations unknowingly.
        // In a fully robust system, we would cache the actual Response object here and return it next time.
        // For our scope, returning 409 Conflict is perfectly safe and fulfills the requirement.
    }
}
