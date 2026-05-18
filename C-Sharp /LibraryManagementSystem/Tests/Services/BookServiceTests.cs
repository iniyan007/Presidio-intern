using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using FluentAssertions;
using Moq;
using Tests.Helpers;

namespace Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository>     _bookRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly BookService               _bookService;

    public BookServiceTests()
    {
        _bookRepoMock     = new Mock<IBookRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _bookService      = new BookService(_bookRepoMock.Object, _categoryRepoMock.Object);
    }

    // ── GetAllBooksAsync Tests ────────────────────────────────
    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            TestDataHelper.CreateBook(1, "978-0001", "Clean Code",       "Robert Martin", 1),
            TestDataHelper.CreateBook(2, "978-0002", "The Great Gatsby", "Fitzgerald",    1),
            TestDataHelper.CreateBook(3, "978-0003", "Sapiens",          "Harari",        2)
        };

        // Add empty copies list to each book
        books.ForEach(b => b.BookCopies = new List<BookCopy>());
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Clean Code");
        result[1].Title.Should().Be("The Great Gatsby");
        result[2].Title.Should().Be("Sapiens");
    }

    [Fact]
    public async Task GetAllBooksAsync_NoBooks_ShouldReturnEmptyList()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Book>());

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ── GetBookByIdAsync Tests ────────────────────────────────
    [Fact]
    public async Task GetBookByIdAsync_ExistingBook_ShouldReturnBook()
    {
        // Arrange
        var book = TestDataHelper.CreateBook();
        book.BookCopies = new List<BookCopy>();
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBookByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Book");
        result.Author.Should().Be("Test Author");
    }

    [Fact]
    public async Task GetBookByIdAsync_NonExistingBook_ShouldReturnNull()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    // ── SearchBooksAsync Tests ────────────────────────────────
    [Fact]
    public async Task SearchBooksAsync_MatchingKeyword_ShouldReturnBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            TestDataHelper.CreateBook(1, "978-0001", "Clean Code", "Robert Martin", 1)
        };
        books.ForEach(b => b.BookCopies = new List<BookCopy>());
        _bookRepoMock.Setup(r => r.SearchAsync("Clean")).ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync("Clean");

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task SearchBooksAsync_NoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.SearchAsync("xyz")).ReturnsAsync(new List<Book>());

        // Act
        var result = await _bookService.SearchBooksAsync("xyz");

        // Assert
        result.Should().BeEmpty();
    }

    // ── GetBooksByCategoryAsync Tests ─────────────────────────
    [Fact]
    public async Task GetBooksByCategoryAsync_ValidCategory_ShouldReturnBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            TestDataHelper.CreateBook(1, "978-0001", "Clean Code",            "Robert Martin", 3),
            TestDataHelper.CreateBook(2, "978-0002", "Pragmatic Programmer",  "Andrew Hunt",   3)
        };
        books.ForEach(b => b.BookCopies = new List<BookCopy>());
        _bookRepoMock.Setup(r => r.GetByCategoryAsync(3)).ReturnsAsync(books);

        // Act
        var result = await _bookService.GetBooksByCategoryAsync(3);

        // Assert
        result.Should().HaveCount(2);
        result.All(b => b.CategoryName == "Fiction").Should().BeTrue();
    }

    [Fact]
    public async Task GetBooksByCategoryAsync_EmptyCategory_ShouldReturnEmptyList()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetByCategoryAsync(99)).ReturnsAsync(new List<Book>());

        // Act
        var result = await _bookService.GetBooksByCategoryAsync(99);

        // Assert
        result.Should().BeEmpty();
    }

    // ── AddBookAsync Tests ────────────────────────────────────
    [Fact]
    public async Task AddBookAsync_ValidInput_ShouldSucceed()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _bookRepoMock.Setup(r => r.AddAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _bookService.AddBookAsync(
            "978-0001", "Clean Code", "Robert Martin", 1);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("Clean Code");
        _bookRepoMock.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task AddBookAsync_InvalidCategory_ShouldThrow()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        // Act
        var act = async () => await _bookService.AddBookAsync(
            "978-0001", "Clean Code", "Robert Martin", 99);

        // Assert
        await act.Should().ThrowAsync<LibraryException>()
                 .WithMessage("*Category not found*");
    }

    [Theory]
    [InlineData("", "Clean Code",  "Robert Martin", 1)]   // Empty ISBN
    [InlineData("978-0001", "",    "Robert Martin", 1)]   // Empty title
    [InlineData("978-0001", "Clean Code", "",       1)]   // Empty author
    [InlineData("978-0001", "Clean Code", "Robert", 0)]   // Invalid category ID
    public async Task AddBookAsync_InvalidInputs_ShouldThrow(
        string isbn, string title, string author, int categoryId)
    {
        // Act
        var act = async () => await _bookService.AddBookAsync(isbn, title, author, categoryId);

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>();
    }

    // ── AddBookCopyAsync Tests ────────────────────────────────
    [Fact]
    public async Task AddBookCopyAsync_ValidBook_ShouldSucceed()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _bookRepoMock.Setup(r => r.AddCopyAsync(It.IsAny<BookCopy>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _bookService.AddBookCopyAsync(1, "Good condition");

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("successfully");
        _bookRepoMock.Verify(r => r.AddCopyAsync(It.IsAny<BookCopy>()), Times.Once);
    }

    [Fact]
    public async Task AddBookCopyAsync_BookNotFound_ShouldThrow()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        // Act
        var act = async () => await _bookService.AddBookCopyAsync(99, "Good condition");

        // Assert
        await act.Should().ThrowAsync<BookNotFoundException>()
                 .WithMessage("*99*");
    }

    [Fact]
    public async Task AddBookCopyAsync_NullRemarks_ShouldSucceed()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _bookRepoMock.Setup(r => r.AddCopyAsync(It.IsAny<BookCopy>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _bookService.AddBookCopyAsync(1, null);

        // Assert
        success.Should().BeTrue();
    }

    [Fact]
    public async Task AddBookCopyAsync_InvalidBookId_ShouldThrow()
    {
        // Act
        var act = async () => await _bookService.AddBookCopyAsync(0, "remarks");

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>()
                 .WithMessage("*positive number*");
    }

    // ── AddCopyAsync Sets Available Status Tests ──────────────
    [Fact]
    public async Task AddBookCopyAsync_ShouldSetStatusAsAvailable()
    {
        // Arrange
        BookCopy? capturedCopy = null;
        _bookRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _bookRepoMock.Setup(r => r.AddCopyAsync(It.IsAny<BookCopy>()))
                     .Callback<BookCopy>(copy => capturedCopy = copy)
                     .Returns(Task.CompletedTask);

        // Act
        await _bookService.AddBookCopyAsync(1, "New copy");

        // Assert
        capturedCopy.Should().NotBeNull();
        capturedCopy!.Status.Should().Be((int)CopyStatus.Available);
        capturedCopy.BookId.Should().Be(1);
        capturedCopy.Remarks.Should().Be("New copy");
    }

    // ── UpdateCopyStatusAsync Tests ───────────────────────────
    [Fact]
    public async Task UpdateCopyStatusAsync_ValidCopy_ShouldSucceed()
    {
        // Arrange
        var copy = TestDataHelper.CreateBookCopy(id: 1, bookId: 1);
        _bookRepoMock.Setup(r => r.GetCopiesByBookIdAsync(1))
                     .ReturnsAsync(new List<BookCopy> { copy });
        _bookRepoMock.Setup(r => r.UpdateCopyAsync(It.IsAny<BookCopy>()))
                     .Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _bookService.UpdateCopyStatusAsync(
            1, CopyStatus.Damaged, "Cover torn");

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("Damaged");
        copy.Status.Should().Be((int)CopyStatus.Damaged);
        copy.Remarks.Should().Be("Cover torn");
    }

    [Fact]
    public async Task UpdateCopyStatusAsync_CopyNotFound_ShouldThrow()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetCopiesByBookIdAsync(99))
                     .ReturnsAsync(new List<BookCopy>());

        // Act
        var act = async () => await _bookService.UpdateCopyStatusAsync(
            99, CopyStatus.Damaged, "remarks");

        // Assert
        await act.Should().ThrowAsync<LibraryException>()
                 .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateCopyStatusAsync_InvalidCopyId_ShouldThrow()
    {
        // Act
        var act = async () => await _bookService.UpdateCopyStatusAsync(
            0, CopyStatus.Damaged, "remarks");

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>()
                 .WithMessage("*positive number*");
    }

    // ── Category Tests ────────────────────────────────────────
    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Fiction"    },
            new Category { Id = 2, Name = "Science"    },
            new Category { Id = 3, Name = "Technology" }
        };
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = await _bookService.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Fiction");
        result[1].Name.Should().Be("Science");
        result[2].Name.Should().Be("Technology");
    }

    [Fact]
    public async Task AddCategoryAsync_ValidName_ShouldSucceed()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
                         .Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _bookService.AddCategoryAsync("Horror");

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("Horror");
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddCategoryAsync_EmptyName_ShouldThrow(string name)
    {
        // Act
        var act = async () => await _bookService.AddCategoryAsync(name);

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>()
                 .WithMessage("*Category*cannot be empty*");
    }

    [Fact]
    public async Task AddCategoryAsync_NameTooLong_ShouldThrow()
    {
        // Arrange
        var longName = new string('A', 101); // 101 chars — exceeds 100 limit

        // Act
        var act = async () => await _bookService.AddCategoryAsync(longName);

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>()
                 .WithMessage("*exceed 100*");
    }

    // ── Available Copies Count Tests ──────────────────────────
    [Fact]
    public async Task GetAllBooksAsync_ShouldCountAvailableCopiesCorrectly()
    {
        // Arrange
        var book = TestDataHelper.CreateBook();
        book.BookCopies = new List<BookCopy>
        {
            TestDataHelper.CreateBookCopy(1, 1, (int)CopyStatus.Available),
            TestDataHelper.CreateBookCopy(2, 1, (int)CopyStatus.Available),
            TestDataHelper.CreateBookCopy(3, 1, (int)CopyStatus.Borrowed),   // Not available
            TestDataHelper.CreateBookCopy(4, 1, (int)CopyStatus.Damaged)     // Not available
        };

        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Book> { book });

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result[0].TotalCopies.Should().Be(4);
        result[0].AvailableCopies.Should().Be(2); // Only Available ones
    }
}