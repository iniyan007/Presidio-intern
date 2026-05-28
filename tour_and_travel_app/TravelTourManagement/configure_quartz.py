import re

file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.API/Program.cs'
with open(file_path, 'r') as f:
    content = f.read()

# Add Quartz usings
if "using Quartz;" not in content:
    content = content.replace("using TravelTourManagement.DataAccess;", "using TravelTourManagement.DataAccess;\nusing Quartz;\nusing TravelTourManagement.Business.Services;")

# Inject Quartz configuration
quartz_config = """// Register Business Services
builder.Services.AddBusinessServices(builder.Configuration);

// Add Quartz services
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("BookingTimeoutJob");

    q.AddJob<BookingTimeoutJob>(opts => opts.WithIdentity(jobKey));
    
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("BookingTimeoutJob-trigger")
        .WithCronSchedule("0 * * ? * *") // Runs every minute
    );
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
"""

content = content.replace("// Register Business Services\nbuilder.Services.AddBusinessServices(builder.Configuration);", quartz_config)

with open(file_path, 'w') as f:
    f.write(content)
