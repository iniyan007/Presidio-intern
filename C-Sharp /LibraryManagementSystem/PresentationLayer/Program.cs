using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using DataAccessLayer.Context;
using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PresentationLayer.Logging;
using PresentationLayer.Menus;

// ── Initialize Logger ─────────────────────────────────────────
FileLogger.Initialize();
FileLogger.Log("Application started.");

// ── Connection String ─────────────────────────────────────────
var connectionString = "Host=localhost;Port=5432;Database=LibraryApp;Username=postgres;Password=iniyanavin";

// ── DI Container ──────────────────────────────────────────────
var services = new ServiceCollection();

services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
services.AddScoped<IMemberRepository,   MemberRepository>();
services.AddScoped<IBookRepository,     BookRepository>();
services.AddScoped<IBorrowRepository,   BorrowRepository>();
services.AddScoped<IFineRepository,     FineRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();

// Services
services.AddScoped<IMemberService, MemberService>();
services.AddScoped<IBookService,   BookService>();
services.AddScoped<IBorrowService, BorrowService>();
services.AddScoped<IFineService,   FineService>();

// Menus
services.AddScoped<MemberMenu>();
services.AddScoped<BookMenu>();
services.AddScoped<BorrowMenu>();
services.AddScoped<FineMenu>();
services.AddScoped<ReportMenu>();

var provider = services.BuildServiceProvider();

// ── Menus ─────────────────────────────────────────────────────
var memberMenu = provider.GetRequiredService<MemberMenu>();
var bookMenu   = provider.GetRequiredService<BookMenu>();
var borrowMenu = provider.GetRequiredService<BorrowMenu>();
var fineMenu   = provider.GetRequiredService<FineMenu>();
var reportMenu = provider.GetRequiredService<ReportMenu>();

// ── Main Menu Loop ────────────────────────────────────────────
bool running = true;
while (running)
{
    Console.Clear();
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║     COMMUNITY LIBRARY SYSTEM         ║");
    Console.WriteLine("╠══════════════════════════════════════╣");
    Console.WriteLine("║  1. Member Management                ║");
    Console.WriteLine("║  2. Book Management                  ║");
    Console.WriteLine("║  3. Borrow Book                      ║");
    Console.WriteLine("║  4. Return Book                      ║");
    Console.WriteLine("║  5. Fine Management                  ║");
    Console.WriteLine("║  6. Reports                          ║");
    Console.WriteLine("║  7. Exit                             ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.Write("\nEnter choice: ");

    var choice = Console.ReadLine()?.Trim();
    FileLogger.Log($"Main Menu Choice: {choice}");

    switch (choice)
    {
        case "1": await memberMenu.ShowAsync();   break;
        case "2": await bookMenu.ShowAsync();     break;
        case "3": await borrowMenu.BorrowAsync(); break;
        case "4": await borrowMenu.ReturnAsync(); break;
        case "5": await fineMenu.ShowAsync();     break;
        case "6": await reportMenu.ShowAsync();   break;
        case "7": running = false;                break;
        default:
            Console.WriteLine("Invalid choice. Press any key...");
            FileLogger.LogError($"Invalid menu choice: {choice}");
            Console.ReadKey();
            break;
    }
}

// ── Session End ───────────────────────────────────────────────
FileLogger.LogSessionEnd();
Console.WriteLine($"\nSession log saved to: {FileLogger.GetLogFilePath()}");
Console.WriteLine("\nThank you for using Community Library System. Goodbye!");