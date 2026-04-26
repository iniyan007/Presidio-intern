using System;
using System.Linq;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=bus_booking;Username=postgres;Password=postgres"));
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var users = db.Users.Select(u => new { u.Email, u.Role, u.IsApproved }).ToList();
    Console.WriteLine("--- Users in DB ---");
    foreach (var u in users)
    {
        Console.WriteLine($"{u.Email} | Role: {u.Role} | Approved: {u.IsApproved}");
    }
}
