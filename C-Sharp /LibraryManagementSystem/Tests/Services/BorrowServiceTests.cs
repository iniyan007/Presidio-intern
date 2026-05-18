using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using Moq;
using Tests.Helpers;

namespace Tests.Services;

public class BorrowServiceTests
{
    private readonly Mock<IBorrowRepository> _borrowRepoMock;
    private readonly Mock<IMemberRepository> _memberRepoMock;
    private readonly Mock<IBookRepository>   _bookRepoMock;
    private readonly Mock<IFineRepository>   _fineRepoMock;
    private readonly LibraryDbContext        _context;
    private readonly BorrowService           _borrowService;

public BorrowServiceTests()
{
    _borrowRepoMock = new Mock<IBorrowRepository>();
    _memberRepoMock = new Mock<IMemberRepository>();
    _bookRepoMock   = new Mock<IBookRepository>();
    _fineRepoMock   = new Mock<IFineRepository>();

    // ── Fix: Suppress transaction warning for InMemory DB ────
    var options = new DbContextOptionsBuilder<LibraryDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options;

    _context = new LibraryDbContext(options);

    _borrowService = new BorrowService(
        _borrowRepoMock.Object,
        _memberRepoMock.Object,
        _bookRepoMock.Object,
        _fineRepoMock.Object,
        _context);
}
    // ── Borrow Book Tests ─────────────────────────────────────
    [Fact]
    public async Task BorrowBookAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var member    = TestDataHelper.CreateMember(membershipTypeId: 1); // Basic
        var bookCopy  = TestDataHelper.CreateBookCopy(status: 0);         // Available

        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(0m);
        _borrowRepoMock.Setup(r => r.GetActiveBorrowsByMemberIdAsync(1)).ReturnsAsync(new List<Borrow>());
        _borrowRepoMock.Setup(r => r.GetActiveBorrowByMemberAndBookAsync(1, 1)).ReturnsAsync((Borrow?)null);
        _bookRepoMock.Setup(r => r.GetAvailableCopyAsync(1)).ReturnsAsync(bookCopy);
        _borrowRepoMock.Setup(r => r.AddAsync(It.IsAny<Borrow>())).Returns(Task.CompletedTask);
        _bookRepoMock.Setup(r => r.UpdateCopyAsync(It.IsAny<BookCopy>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _borrowService.BorrowBookAsync(1, 1);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("borrowed successfully");
        _borrowRepoMock.Verify(r => r.AddAsync(It.IsAny<Borrow>()), Times.Once);
    }

    [Fact]
    public async Task BorrowBookAsync_InactiveMember_ShouldThrow()
    {
        // Arrange
        var member = TestDataHelper.CreateMember(status: 1); // Inactive
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        // Act
        var act = async () => await _borrowService.BorrowBookAsync(1, 1);

        // Assert
        await act.Should().ThrowAsync<InactiveMemberException>()
                 .WithMessage("*inactive*");
    }

    [Fact]
    public async Task BorrowBookAsync_UnpaidFineExceedsLimit_ShouldThrow()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(600m); // Over ₹500

        // Act
        var act = async () => await _borrowService.BorrowBookAsync(1, 1);

        // Assert
        await act.Should().ThrowAsync<UnpaidFineException>()
                 .WithMessage("*₹600*");
    }

    [Fact]
    public async Task BorrowBookAsync_BorrowLimitReached_ShouldThrow()
    {
        // Arrange
        var member       = TestDataHelper.CreateMember(membershipTypeId: 1); // Basic: max 2
        var activeBorrows = new List<Borrow>
        {
            TestDataHelper.CreateBorrow(id: 1),
            TestDataHelper.CreateBorrow(id: 2)  // Already 2 — limit reached
        };

        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(0m);
        _borrowRepoMock.Setup(r => r.GetActiveBorrowsByMemberIdAsync(1)).ReturnsAsync(activeBorrows);

        // Act
        var act = async () => await _borrowService.BorrowBookAsync(1, 1);

        // Assert
        await act.Should().ThrowAsync<BorrowLimitExceededException>()
                 .WithMessage("*2*");
    }

    [Fact]
    public async Task BorrowBookAsync_DuplicateActiveBorrow_ShouldThrow()
    {
        // Arrange
        var member          = TestDataHelper.CreateMember();
        var duplicateBorrow = TestDataHelper.CreateBorrow();

        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(0m);
        _borrowRepoMock.Setup(r => r.GetActiveBorrowsByMemberIdAsync(1)).ReturnsAsync(new List<Borrow>());
        _borrowRepoMock.Setup(r => r.GetActiveBorrowByMemberAndBookAsync(1, 1)).ReturnsAsync(duplicateBorrow);

        // Act
        var act = async () => await _borrowService.BorrowBookAsync(1, 1);

        // Assert
        await act.Should().ThrowAsync<DuplicateBorrowException>();
    }

    [Fact]
    public async Task BorrowBookAsync_NoAvailableCopy_ShouldThrow()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();

        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(0m);
        _borrowRepoMock.Setup(r => r.GetActiveBorrowsByMemberIdAsync(1)).ReturnsAsync(new List<Borrow>());
        _borrowRepoMock.Setup(r => r.GetActiveBorrowByMemberAndBookAsync(1, 1)).ReturnsAsync((Borrow?)null);
        _bookRepoMock.Setup(r => r.GetAvailableCopyAsync(1)).ReturnsAsync((BookCopy?)null);

        // Act
        var act = async () => await _borrowService.BorrowBookAsync(1, 1);

        // Assert
        await act.Should().ThrowAsync<BookNotAvailableException>();
    }

    [Fact]
    public async Task BorrowBookAsync_MemberNotFound_ShouldThrow()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Member?)null);

        // Act
        var act = async () => await _borrowService.BorrowBookAsync(99, 1);

        // Assert
        await act.Should().ThrowAsync<MemberNotFoundException>()
                 .WithMessage("*99*");
    }

    // ── Return Book Tests ─────────────────────────────────────
    [Fact]
    public async Task ReturnBookAsync_OnTime_ShouldSucceedWithNoFine()
    {
        // Arrange
        var borrow   = TestDataHelper.CreateBorrow(dueDaysFromNow: 3); // Not overdue
        var bookCopy = TestDataHelper.CreateBookCopy();

        borrow.BookCopy = bookCopy;

        _borrowRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(borrow);
        _borrowRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Borrow>())).Returns(Task.CompletedTask);
        _bookRepoMock.Setup(r => r.UpdateCopyAsync(It.IsAny<BookCopy>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _borrowService.ReturnBookAsync(1);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("No fine");
    }

    [Fact]
    public async Task ReturnBookAsync_Overdue_ShouldCalculateFine()
    {
        // Arrange
        var borrow   = TestDataHelper.CreateOverdueBorrow(overdueDays: 5);
        var bookCopy = TestDataHelper.CreateBookCopy();
        borrow.BookCopy = bookCopy;

        _borrowRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(borrow);
        _borrowRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Borrow>())).Returns(Task.CompletedTask);
        _bookRepoMock.Setup(r => r.UpdateCopyAsync(It.IsAny<BookCopy>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _borrowService.ReturnBookAsync(1);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("₹50"); // 5 days * ₹10
    }

    [Fact]
    public async Task ReturnBookAsync_AlreadyReturned_ShouldThrow()
    {
        // Arrange
        var borrow = TestDataHelper.CreateBorrow(status: 1); // Already returned
        _borrowRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(borrow);

        // Act
        var act = async () => await _borrowService.ReturnBookAsync(1);

        // Assert
        await act.Should().ThrowAsync<AlreadyReturnedException>();
    }
}