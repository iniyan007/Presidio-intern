using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using DataAccessLayer.Enums;

namespace PresentationLayer.Menus;

public class BookMenu
{
    private readonly IBookService _bookService;

    public BookMenu(IBookService bookService)
    {
        _bookService = bookService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            ConsoleHelper.PrintHeader("BOOK MANAGEMENT");
            Console.WriteLine("  1. Add New Book");
            Console.WriteLine("  2. Add Book Copy");
            Console.WriteLine("  3. View All Books");
            Console.WriteLine("  4. Search Books");
            Console.WriteLine("  5. View Books by Category");
            Console.WriteLine("  6. Update Copy Status");
            Console.WriteLine("  7. Add Category");
            Console.WriteLine("  8. View All Categories");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Choice: ");

            switch (Console.ReadLine())
            {
                case "1": await AddBookAsync();          break;
                case "2": await AddBookCopyAsync();      break;
                case "3": await ViewAllBooksAsync();     break;
                case "4": await SearchBooksAsync();      break;
                case "5": await ViewByCategoryAsync();   break;
                case "6": await UpdateCopyStatusAsync(); break;
                case "7": await AddCategoryAsync();      break;
                case "8": await ViewCategoriesAsync();   break;
                case "0": back = true;                   break;
                default:
                    ConsoleHelper.PrintError("Invalid choice. Please enter a number between 0-8.");
                    ConsoleHelper.Pause();
                    break;
            }
        }
    }

    private async Task AddBookAsync()
    {
        ConsoleHelper.PrintHeader("ADD NEW BOOK");
        try
        {
            await ViewCategoriesAsync(pause: false);

            var isbn       = ConsoleHelper.ReadInput("ISBN");
            var title      = ConsoleHelper.ReadInput("Title");
            var author     = ConsoleHelper.ReadInput("Author");
            var categoryId = ConsoleHelper.ReadInt("Category ID");

            var (success, message) = await _bookService.AddBookAsync(isbn, title, author, categoryId);
            if (success) ConsoleHelper.PrintSuccess(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task AddBookCopyAsync()
    {
        ConsoleHelper.PrintHeader("ADD BOOK COPY");
        try
        {
            var bookId  = ConsoleHelper.ReadInt("Book ID");
            var remarks = ConsoleHelper.ReadInput("Remarks (optional, press Enter to skip)");

            var (success, message) = await _bookService.AddBookCopyAsync(
                bookId, string.IsNullOrWhiteSpace(remarks) ? null : remarks);

            if (success) ConsoleHelper.PrintSuccess(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task ViewAllBooksAsync()
    {
        ConsoleHelper.PrintHeader("ALL BOOKS");
        try
        {
            var books = await _bookService.GetAllBooksAsync();

            if (!books.Any())
            {
                ConsoleHelper.PrintInfo("No books found.");
                ConsoleHelper.Pause();
                return;
            }

            PrintBooksTable(books);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task SearchBooksAsync()
    {
        ConsoleHelper.PrintHeader("SEARCH BOOKS");
        try
        {
            var keyword = ConsoleHelper.ReadInput("Search (title / author / category)");

            if (string.IsNullOrWhiteSpace(keyword))
                throw new InvalidInputException("Keyword", "Search keyword cannot be empty.");

            var books = await _bookService.SearchBooksAsync(keyword);

            if (!books.Any()) ConsoleHelper.PrintInfo("No books found matching your search.");
            else              PrintBooksTable(books);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task ViewByCategoryAsync()
    {
        ConsoleHelper.PrintHeader("BOOKS BY CATEGORY");
        try
        {
            await ViewCategoriesAsync(pause: false);

            var categoryId = ConsoleHelper.ReadInt("Category ID");
            var books      = await _bookService.GetBooksByCategoryAsync(categoryId);

            if (!books.Any()) ConsoleHelper.PrintInfo("No books found in this category.");
            else              PrintBooksTable(books);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task UpdateCopyStatusAsync()
    {
        ConsoleHelper.PrintHeader("UPDATE COPY STATUS");
        try
        {
            var copyId = ConsoleHelper.ReadInt("Book Copy ID");

            Console.WriteLine("  1. Available");
            Console.WriteLine("  2. Damaged");
            Console.WriteLine("  3. Lost");
            var choice = ConsoleHelper.ReadInput("Select status (1/2/3)");

            var status = choice switch
            {
                "1" => CopyStatus.Available,
                "2" => CopyStatus.Damaged,
                "3" => CopyStatus.Lost,
                _   => throw new InvalidInputException("Status", "Please enter 1, 2, or 3.")
            };

            var remarks        = ConsoleHelper.ReadInput("Remarks (optional, press Enter to skip)");
            var (success, msg) = await _bookService.UpdateCopyStatusAsync(
                copyId, status, string.IsNullOrWhiteSpace(remarks) ? null : remarks);

            if (success) ConsoleHelper.PrintSuccess(msg);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task AddCategoryAsync()
    {
        ConsoleHelper.PrintHeader("ADD CATEGORY");
        try
        {
            var name           = ConsoleHelper.ReadInput("Category Name");
            var (success, msg) = await _bookService.AddCategoryAsync(name);

            if (success) ConsoleHelper.PrintSuccess(msg);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task ViewCategoriesAsync(bool pause = true)
    {
        try
        {
            var categories = await _bookService.GetAllCategoriesAsync();

            if (!categories.Any())
            {
                ConsoleHelper.PrintInfo("No categories found.");
                return;
            }

            Console.WriteLine($"\n  {"ID",-5} {"Category Name",-20}");
            Console.WriteLine($"  {"─",-5} {"─",-20}");
            foreach (var c in categories)
                Console.WriteLine($"  {c.Id,-5} {c.Name,-20}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        if (pause) ConsoleHelper.Pause();
    }

    private static void PrintBooksTable(List<BusinessLayer.DTOs.BookDto> books)
    {
        Console.WriteLine($"\n  {"ID",-5} {"Title",-30} {"Author",-20} {"Category",-15} {"Total",-7} {"Available",-10}");
        Console.WriteLine($"  {"─",-5} {"─",-30} {"─",-20} {"─",-15} {"─",-7} {"─",-10}");
        foreach (var b in books)
            Console.WriteLine($"  {b.Id,-5} {b.Title,-30} {b.Author,-20} {b.CategoryName,-15} {b.TotalCopies,-7} {b.AvailableCopies,-10}");
    }
}