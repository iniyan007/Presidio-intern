using BusinessLayer.Interfaces;

namespace PresentationLayer.Menus;

public class ReportMenu
{
    private readonly IBorrowService _borrowService;
    private readonly IFineService   _fineService;
    private readonly IBookService   _bookService;
    private readonly IMemberService _memberService;

    public ReportMenu(
        IBorrowService borrowService,
        IFineService   fineService,
        IBookService   bookService,
        IMemberService memberService)
    {
        _borrowService = borrowService;
        _fineService   = fineService;
        _bookService   = bookService;
        _memberService = memberService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            ConsoleHelper.PrintHeader("REPORTS");
            Console.WriteLine("  1. Currently Borrowed Books");
            Console.WriteLine("  2. Overdue Books");
            Console.WriteLine("  3. Member Borrowing Summary");
            Console.WriteLine("  4. Available Books by Category");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Choice: ");

            switch (Console.ReadLine())
            {
                case "1": await CurrentlyBorrowedAsync();        break;
                case "2": await OverdueBooksAsync();             break;
                case "3": await MemberSummaryAsync();            break;
                case "4": await AvailableBooksByCategoryAsync(); break;
                case "0": back = true;                           break;
                default:
                    ConsoleHelper.PrintError("Invalid choice. Please enter a number between 0-4.");
                    ConsoleHelper.Pause();
                    break;
            }
        }
    }

    private async Task CurrentlyBorrowedAsync()
    {
        ConsoleHelper.PrintHeader("CURRENTLY BORROWED BOOKS");
        try
        {
            var borrows = await _borrowService.GetActiveBorrowsAsync();

            if (!borrows.Any())
            {
                ConsoleHelper.PrintInfo("No active borrows at the moment.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  {"ID",-5} {"Member",-20} {"Book Title",-30} {"Due Date",-12} {"Overdue",-10}");
            Console.WriteLine($"  {"─",-5} {"─",-20} {"─",-30} {"─",-12} {"─",-10}");

            foreach (var b in borrows)
            {
                var overdue = b.IsOverdue ? $"Yes ({b.OverdueDays}d)" : "No";
                Console.WriteLine($"  {b.Id,-5} {b.MemberName,-20} {b.BookTitle,-30} {b.DueDate,-12} {overdue,-10}");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task OverdueBooksAsync()
    {
        ConsoleHelper.PrintHeader("OVERDUE BOOKS");
        try
        {
            var borrows = await _borrowService.GetOverdueBorrowsAsync();

            if (!borrows.Any())
            {
                ConsoleHelper.PrintInfo("No overdue books at the moment.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  {"ID",-5} {"Member",-20} {"Book Title",-30} {"Due Date",-12} {"Days Late",-10} {"Fine",-10}");
            Console.WriteLine($"  {"─",-5} {"─",-20} {"─",-30} {"─",-12} {"─",-10} {"─",-10}");

            foreach (var b in borrows)
                Console.WriteLine($"  {b.Id,-5} {b.MemberName,-20} {b.BookTitle,-30} {b.DueDate,-12} {b.OverdueDays,-10} ₹{b.OverdueDays * 10,-9}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task MemberSummaryAsync()
    {
        ConsoleHelper.PrintHeader("MEMBER BORROWING SUMMARY");
        try
        {
            var memberId = ConsoleHelper.ReadInt("Member ID");
            var summary  = await _borrowService.GetMemberBorrowingSummaryAsync(memberId);

            if (summary is null)
            {
                ConsoleHelper.PrintError("Member not found.");
                ConsoleHelper.Pause();
                return;
            }

            ConsoleHelper.PrintInfo($"Member         : {summary.MemberName}");
            ConsoleHelper.PrintInfo($"Active Borrows : {summary.ActiveBorrowings}");
            ConsoleHelper.PrintInfo($"Returned Books : {summary.ReturnedBorrowings}");
            ConsoleHelper.PrintInfo($"Unpaid Fine    : ₹{summary.TotalUnpaidFine}");

            if (summary.ActiveBooks.Any())
            {
                Console.WriteLine($"\n  {"Book Title",-30} {"Due Date",-12} {"Overdue",-10}");
                Console.WriteLine($"  {"─",-30} {"─",-12} {"─",-10}");
                foreach (var b in summary.ActiveBooks)
                {
                    var overdue = b.IsOverdue ? $"Yes ({b.OverdueDays}d)" : "No";
                    Console.WriteLine($"  {b.BookTitle,-30} {b.DueDate,-12} {overdue,-10}");
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task AvailableBooksByCategoryAsync()
    {
        ConsoleHelper.PrintHeader("AVAILABLE BOOKS BY CATEGORY");
        try
        {
            var categories = await _bookService.GetAllCategoriesAsync();

            if (!categories.Any())
            {
                ConsoleHelper.PrintInfo("No categories found.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  {"ID",-5} {"Category",-20}");
            Console.WriteLine($"  {"─",-5} {"─",-20}");
            foreach (var c in categories)
                Console.WriteLine($"  {c.Id,-5} {c.Name,-20}");

            var categoryId = ConsoleHelper.ReadInt("\n  Category ID");
            var books      = await _bookService.GetBooksByCategoryAsync(categoryId);
            var available  = books.Where(b => b.AvailableCopies > 0).ToList();

            if (!available.Any())
            {
                ConsoleHelper.PrintInfo("No available books in this category.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  {"ID",-5} {"Title",-30} {"Author",-20} {"Available",-10}");
            Console.WriteLine($"  {"─",-5} {"─",-30} {"─",-20} {"─",-10}");
            foreach (var b in available)
                Console.WriteLine($"  {b.Id,-5} {b.Title,-30} {b.Author,-20} {b.AvailableCopies,-10}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }
}