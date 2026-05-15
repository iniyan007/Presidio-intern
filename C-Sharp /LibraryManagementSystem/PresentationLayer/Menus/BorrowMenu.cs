using BusinessLayer.Interfaces;

namespace PresentationLayer.Menus;

public class BorrowMenu
{
    private readonly IBorrowService _borrowService;

    public BorrowMenu(IBorrowService borrowService)
    {
        _borrowService = borrowService;
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
            var borrowId = ConsoleHelper.ReadInt("Borrow Record ID");

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