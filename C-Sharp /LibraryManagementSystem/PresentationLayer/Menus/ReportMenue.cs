using BusinessLayer.Interfaces;
using PresentationLayer.Logging;

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
            Console.WriteLine("  5. Members with Pending Fines");   
            Console.WriteLine("  6. Most Borrowed Books");           
            Console.WriteLine("  7. Member Borrowing History");      
            Console.WriteLine("  0. Back");
            Console.Write("\n  Choice: ");

            switch (Console.ReadLine())
            {
                case "1": await CurrentlyBorrowedAsync();        break;
                case "2": await OverdueBooksAsync();             break;
                case "3": await MemberSummaryAsync();            break;
                case "4": await AvailableBooksByCategoryAsync(); break;
                case "5": await MembersWithPendingFinesAsync();  break;  
                case "6": await MostBorrowedBooksAsync();        break;  
                case "7": await MemberBorrowingHistoryAsync();   break;  
                case "0": back = true;                           break;
                default:
                    ConsoleHelper.PrintError("Invalid choice. Please enter a number between 0-7.");
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
                Console.ForegroundColor = b.IsOverdue ? ConsoleColor.Red : ConsoleColor.White;
                Console.WriteLine($"  {b.Id,-5} {b.MemberName,-20} {b.BookTitle,-30} {b.DueDate,-12} {overdue,-10}");
                Console.ResetColor();
                FileLogger.LogInfo($"Borrowed — ID: {b.Id} | Member: {b.MemberName} | Book: {b.BookTitle} | Due: {b.DueDate} | Overdue: {overdue}");
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
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  {b.Id,-5} {b.MemberName,-20} {b.BookTitle,-30} {b.DueDate,-12} {b.OverdueDays,-10} ₹{b.OverdueDays * 10,-9}");
                Console.ResetColor();
                FileLogger.LogInfo($"Overdue — ID: {b.Id} | Member: {b.MemberName} | Book: {b.BookTitle} | Days Late: {b.OverdueDays} | Fine: ₹{b.OverdueDays * 10}");
            }
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

            FileLogger.LogInfo($"Summary — Member: {summary.MemberName} | Active: {summary.ActiveBorrowings} | Returned: {summary.ReturnedBorrowings} | Fine: ₹{summary.TotalUnpaidFine}");

            if (summary.ActiveBooks.Any())
            {
                Console.WriteLine($"\n  {"Book Title",-30} {"Due Date",-12} {"Overdue",-10}");
                Console.WriteLine($"  {"─",-30} {"─",-12} {"─",-10}");
                foreach (var b in summary.ActiveBooks)
                {
                    var overdue = b.IsOverdue ? $"Yes ({b.OverdueDays}d)" : "No";
                    Console.ForegroundColor = b.IsOverdue ? ConsoleColor.Red : ConsoleColor.White;
                    Console.WriteLine($"  {b.BookTitle,-30} {b.DueDate,-12} {overdue,-10}");
                    Console.ResetColor();
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
            {
                Console.WriteLine($"  {b.Id,-5} {b.Title,-30} {b.Author,-20} {b.AvailableCopies,-10}");
                FileLogger.LogInfo($"Available — ID: {b.Id} | Title: {b.Title} | Copies: {b.AvailableCopies}");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task MembersWithPendingFinesAsync()
    {
        ConsoleHelper.PrintHeader("MEMBERS WITH PENDING FINES");
        try
        {
            var members     = await _memberService.GetAllMembersAsync();
            var fineMembers = new List<(string Name, string Phone, decimal Fine)>();

            foreach (var member in members)
            {
                var unpaid = await _fineService.GetUnpaidFineAsync(member.Id);
                if (unpaid > 0)
                    fineMembers.Add((member.Name, member.Phone, unpaid));
            }

            if (!fineMembers.Any())
            {
                ConsoleHelper.PrintInfo("No members with pending fines.");
                ConsoleHelper.Pause();
                return;
            }

            fineMembers = fineMembers.OrderByDescending(f => f.Fine).ToList();

            Console.WriteLine($"\n  {"Name",-20} {"Phone",-15} {"Unpaid Fine",-15} {"Alert",-15}");
            Console.WriteLine($"  {"─",-20} {"─",-15} {"─",-15} {"─",-15}");

            foreach (var (name, phone, fine) in fineMembers)
            {
                var alert = fine > 500 ? "⚠ Blocked" : "Active";
                Console.ForegroundColor = fine > 500 ? ConsoleColor.Red : ConsoleColor.Yellow;
                Console.WriteLine($"  {name,-20} {phone,-15} ₹{fine,-14} {alert,-15}");
                Console.ResetColor();
                FileLogger.LogInfo($"Pending Fine — Member: {name} | Phone: {phone} | Fine: ₹{fine} | Alert: {alert}");
            }

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Total members with pending fines: {fineMembers.Count}");
            ConsoleHelper.PrintInfo($"Total pending amount            : ₹{fineMembers.Sum(f => f.Fine)}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task MostBorrowedBooksAsync()
    {
        ConsoleHelper.PrintHeader("MOST BORROWED BOOKS");
        try
        {
            var allBorrows = await _borrowService.GetAllBorrowsAsync();

            if (!allBorrows.Any())
            {
                ConsoleHelper.PrintInfo("No borrow records found.");
                ConsoleHelper.Pause();
                return;
            }

            var mostBorrowed = allBorrows
                .GroupBy(b => new { b.BookTitle, b.BookAuthor })
                .Select(g => new
                {
                    g.Key.BookTitle,
                    g.Key.BookAuthor,
                    TotalBorrows   = g.Count(),
                    ActiveBorrows  = g.Count(b => b.Status == DataAccessLayer.Enums.BorrowStatus.Borrowed),
                    ReturnedBorrows = g.Count(b => b.Status == DataAccessLayer.Enums.BorrowStatus.Returned)
                })
                .OrderByDescending(b => b.TotalBorrows)
                .Take(10)  // Top 10
                .ToList();

            Console.WriteLine($"\n  {"Rank",-6} {"Book Title",-30} {"Author",-20} {"Total",-8} {"Active",-8} {"Returned",-10}");
            Console.WriteLine($"  {"─",-6} {"─",-30} {"─",-20} {"─",-8} {"─",-8} {"─",-10}");

            int rank = 1;
            foreach (var book in mostBorrowed)
            {
                Console.ForegroundColor = rank == 1 ? ConsoleColor.Yellow :
                                          rank == 2 ? ConsoleColor.Cyan   :
                                          rank == 3 ? ConsoleColor.Green  :
                                          ConsoleColor.White;

                Console.WriteLine($"  {rank,-6} {book.BookTitle,-30} {book.BookAuthor,-20} {book.TotalBorrows,-8} {book.ActiveBorrows,-8} {book.ReturnedBorrows,-10}");
                Console.ResetColor();
                FileLogger.LogInfo($"Rank {rank} — Book: {book.BookTitle} | Total: {book.TotalBorrows} | Active: {book.ActiveBorrows} | Returned: {book.ReturnedBorrows}");
                rank++;
            }

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Showing top {mostBorrowed.Count} most borrowed books.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task MemberBorrowingHistoryAsync()
    {
        ConsoleHelper.PrintHeader("MEMBER BORROWING HISTORY");
        try
        {
            var memberId = ConsoleHelper.ReadInt("Member ID");
            var member   = await _memberService.GetMemberByIdAsync(memberId);

            if (member is null)
            {
                ConsoleHelper.PrintError("Member not found.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Member  : {member.Name}");
            Console.WriteLine($"  Phone   : {member.Phone}");
            Console.WriteLine($"  Email   : {member.Email}");
            Console.WriteLine($"  Type    : {member.MembershipType}");
            Console.WriteLine($"  Status  : {member.Status}");
            Console.ResetColor();

            var history = await _borrowService.GetBorrowsByMemberAsync(memberId);

            if (!history.Any())
            {
                ConsoleHelper.PrintInfo("No borrowing history found for this member.");
                ConsoleHelper.Pause();
                return;
            }
            var active   = history.Where(b => b.Status == DataAccessLayer.Enums.BorrowStatus.Borrowed).ToList();
            var returned = history.Where(b => b.Status == DataAccessLayer.Enums.BorrowStatus.Returned).ToList();
            if (active.Any())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ── Active Borrows ({active.Count}) ──────────────────────────────");
                Console.ResetColor();
                Console.WriteLine($"\n  {"ID",-5} {"Book Title",-30} {"Borrowed On",-14} {"Due Date",-12} {"Status",-15}");
                Console.WriteLine($"  {"─",-5} {"─",-30} {"─",-14} {"─",-12} {"─",-15}");

                foreach (var b in active)
                {
                    var status = b.IsOverdue ? $"Overdue({b.OverdueDays}d)" : "On Time";
                    Console.ForegroundColor = b.IsOverdue ? ConsoleColor.Red : ConsoleColor.Green;
                    Console.WriteLine($"  {b.Id,-5} {b.BookTitle,-30} {b.DateOfBorrow,-14} {b.DueDate,-12} {status,-15}");
                    Console.ResetColor();
                    FileLogger.LogInfo($"Active — ID: {b.Id} | Book: {b.BookTitle} | Due: {b.DueDate} | Status: {status}");
                }
            }
            if (returned.Any())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ── Return History ({returned.Count}) ─────────────────────────────");
                Console.ResetColor();
                Console.WriteLine($"\n  {"ID",-5} {"Book Title",-30} {"Borrowed On",-14} {"Returned On",-14} {"Fine",-10}");
                Console.WriteLine($"  {"─",-5} {"─",-30} {"─",-14} {"─",-14} {"─",-10}");

                foreach (var b in returned)
                {
                    var fine = b.FineAmount > 0 ? $"₹{b.FineAmount}" : "None";
                    Console.ForegroundColor = b.FineAmount > 0 ? ConsoleColor.Yellow : ConsoleColor.White;
                    Console.WriteLine($"  {b.Id,-5} {b.BookTitle,-30} {b.DateOfBorrow,-14} {b.DateOfReturn,-14} {fine,-10}");
                    Console.ResetColor();
                    FileLogger.LogInfo($"Returned — ID: {b.Id} | Book: {b.BookTitle} | Returned: {b.DateOfReturn} | Fine: {fine}");
                }
            }

            var totalFine = history.Sum(b => b.FineAmount);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  ── Summary ─────────────────────────────────────────");
            Console.ResetColor();
            ConsoleHelper.PrintInfo($"Total Borrows  : {history.Count}");
            ConsoleHelper.PrintInfo($"Active         : {active.Count}");
            ConsoleHelper.PrintInfo($"Returned       : {returned.Count}");
            ConsoleHelper.PrintInfo($"Total Fines    : ₹{totalFine}");

            FileLogger.LogInfo($"History Summary — Member: {member.Name} | Total: {history.Count} | Active: {active.Count} | Returned: {returned.Count} | Total Fine: ₹{totalFine}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }
}