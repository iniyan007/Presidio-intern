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
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        
        if (cachedValue != null)
        {
            context.Result = new ConflictObjectResult(new { message = "Duplicate request detected. This operation has already been processed or is currently processing." });
            return;
        }
        await _cache.SetStringAsync(cacheKey, "PROCESSING", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });
        var executedContext = await next();
    }
}
