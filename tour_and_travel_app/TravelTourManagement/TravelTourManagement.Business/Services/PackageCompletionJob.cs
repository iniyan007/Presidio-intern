using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class PackageCompletionJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PackageCompletionJob> _logger;

    public PackageCompletionJob(IServiceProvider serviceProvider, ILogger<PackageCompletionJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var cancellationToken = context.CancellationToken;
            
            using var scope = _serviceProvider.CreateScope();
            var packageRepository = scope.ServiceProvider.GetRequiredService<IPackageRepository>();
            var pricingRepository = scope.ServiceProvider.GetRequiredService<IRepository<PackageSeasonalPricing, Guid>>();
            var allPackages = await packageRepository.GetAllAsync();
            var publishedPackages = allPackages.Where(p => p.Status == PackageStatus.Published).ToList();

            if (!publishedPackages.Any())
                return;

            int completedCount = 0;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var package in publishedPackages)
            {
                var allPricings = await pricingRepository.GetAllAsync();
                var packagePricings = allPricings.Where(p => p.PackageId == package.Id && p.IsActive).ToList();

                if (packagePricings.Any() && packagePricings.All(p => p.EndDate < today))
                {
                    package.Status = PackageStatus.Completed;
                    package.UpdatedAt = DateTime.UtcNow;
                    
                    await packageRepository.UpdateAsync(package, cancellationToken);
                    completedCount++;
                    
                    _logger.LogInformation($"[Quartz Job] Automatically marked package {package.Id} ('{package.Title}') as Completed.");
                }
            }
            
            if (completedCount > 0)
            {
                _logger.LogInformation($"[Quartz Job] Finished processing. {completedCount} packages marked as Completed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Quartz Job] An error occurred while processing package completion.");
        }
    }
}
