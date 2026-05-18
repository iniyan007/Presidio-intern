using BusinessLayer.Interfaces;
using PresentationLayer.Logging;

namespace PresentationLayer.Menus;

public class BorrowMenu
{
    private readonly IBorrowService _borrowService;
    private readonly IFineService   _fineService;

    public BorrowMenu(IBorrowService borrowService, IFineService fineService)
    {
        _borrowService = borrowService;
        _fineService   = fineService;
    }

    public async Task BorrowAsync()
    {
        ConsoleHelper.PrintHeader("BORROW BOOK");
        try
        {
            var memberId = ConsoleHelper.ReadInt("Member ID");
            var bookId   = ConsoleHelper.ReadInt("Book ID");

            var (success, message) = await _borrowService.BorrowBookAsync(memberId, bookId);
            if (success) ConsoleHelper.PrintSuccess(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    public async Task ReturnAsync()
    {
        ConsoleHelper.PrintHeader("RETURN BOOK");
        try
        {
            var memberId      = ConsoleHelper.ReadInt("Member ID");
            var activeBorrows = await _borrowService.GetActiveBorrowsByMemberAsync(memberId);

            if (!activeBorrows.Any())
            {
                ConsoleHelper.PrintInfo("No active borrows found for this member.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"  {"Borrow ID",-12} {"Book Title",-35} {"Author",-20} {"Borrowed On",-14} {"Due Date",-12} {"Status",-10}");
            Console.WriteLine($"  {"─",-12} {"─",-35} {"─",-20} {"─",-14} {"─",-12} {"─",-10}");

            foreach (var b in activeBorrows)
            {
                var status = b.IsOverdue ? $"Overdue({b.OverdueDays}d)" : "On Time";
                Console.ForegroundColor = b.IsOverdue ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine($"  {b.Id,-12} {b.BookTitle,-35} {b.BookAuthor,-20} {b.DateOfBorrow,-14} {b.DueDate,-12} {status,-10}");
                Console.ResetColor();
                FileLogger.LogInfo($"Borrow ID: {b.Id} | Book: {b.BookTitle} | Due: {b.DueDate} | {status}");
            }

            Console.WriteLine();

            var borrowId = ConsoleHelper.ReadInt("Enter Borrow ID to Return");

            var selectedBorrow = activeBorrows.FirstOrDefault(b => b.Id == borrowId);
            if (selectedBorrow is null)
            {
                ConsoleHelper.PrintError($"Borrow ID {borrowId} does not belong to Member ID {memberId} or is already returned.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Book Title : {selectedBorrow.BookTitle}");
            ConsoleHelper.PrintInfo($"Author     : {selectedBorrow.BookAuthor}");
            ConsoleHelper.PrintInfo($"Borrowed On: {selectedBorrow.DateOfBorrow}");
            ConsoleHelper.PrintInfo($"Due Date   : {selectedBorrow.DueDate}");

            if (selectedBorrow.IsOverdue)
            {
                var expectedFine = selectedBorrow.OverdueDays * 10m;

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ⚠ This book is overdue by {selectedBorrow.OverdueDays} day(s).");
                Console.WriteLine($"  ⚠ Fine Amount to Pay : ₹{expectedFine}");
                Console.ResetColor();
                FileLogger.LogError($"Overdue return — {selectedBorrow.OverdueDays} days late. Fine: ₹{expectedFine}");

                while (true)
                {
                    var amountPaid = ConsoleHelper.ReadDecimal("Enter Fine Amount to Pay");

                    if (amountPaid == expectedFine)
                    {
                        ConsoleHelper.PrintSuccess($"Fine of ₹{expectedFine} paid successfully.");
                        FileLogger.LogSuccess($"Fine paid: ₹{amountPaid} by Member ID {memberId}");

                        var (finePaid, fineMsg) = await _fineService.PayFineForBorrowAsync(memberId, borrowId, amountPaid);
                        if (!finePaid)
                        {
                            ConsoleHelper.PrintError(fineMsg);
                            ConsoleHelper.Pause();
                            return;
                        }
                        break;
                    }
                    else if (amountPaid < expectedFine)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n  ⚠ Amount too low. You must pay the full fine of ₹{expectedFine} to return the book.");
                        Console.ResetColor();
                        FileLogger.LogError($"Insufficient: ₹{amountPaid} entered, ₹{expectedFine} required.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n  ⚠ Amount too high. Fine is exactly ₹{expectedFine}. Please enter the exact amount.");
                        Console.ResetColor();
                        FileLogger.LogError($"Excess: ₹{amountPaid} entered, ₹{expectedFine} required.");
                    }
                }
            }
            else
            {
                ConsoleHelper.PrintInfo("No fine. Book is being returned on time.");
            }

            Console.WriteLine();
            Console.Write("  Confirm return? (y/n): ");
            var confirm = Console.ReadLine()?.Trim().ToLower();
            FileLogger.LogInput("Confirm Return", confirm ?? "n");

            if (confirm != "y")
            {
                ConsoleHelper.PrintInfo("Return cancelled.");
                ConsoleHelper.Pause();
                return;
            }

            var (success, message) = await _borrowService.ReturnBookAsync(borrowId);
            if (success) ConsoleHelper.PrintSuccess(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }
}