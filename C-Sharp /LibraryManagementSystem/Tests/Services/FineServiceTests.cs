using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using FluentAssertions;
using Moq;
using Tests.Helpers;

namespace Tests.Services;

public class FineServiceTests
{
    private readonly Mock<IFineRepository>   _fineRepoMock;
    private readonly Mock<IMemberRepository> _memberRepoMock;
    private readonly FineService             _fineService;

    public FineServiceTests()
    {
        _fineRepoMock   = new Mock<IFineRepository>();
        _memberRepoMock = new Mock<IMemberRepository>();
        _fineService    = new FineService(_fineRepoMock.Object, _memberRepoMock.Object);
    }

    // ── GetUnpaidFineAsync Tests ──────────────────────────────
    [Fact]
    public async Task GetUnpaidFineAsync_MemberWithFine_ShouldReturnFineAmount()
    {
        // Arrange
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(150m);

        // Act
        var result = await _fineService.GetUnpaidFineAsync(1);

        // Assert
        result.Should().Be(150m);
    }

    [Fact]
    public async Task GetUnpaidFineAsync_MemberWithNoFine_ShouldReturnZero()
    {
        // Arrange
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(0m);

        // Act
        var result = await _fineService.GetUnpaidFineAsync(1);

        // Assert
        result.Should().Be(0m);
    }

    // ── GetFineSummaryAsync Tests ─────────────────────────────
    [Fact]
    public async Task GetFineSummaryAsync_ValidMember_ShouldReturnSummary()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(100m);
        _fineRepoMock.Setup(r => r.GetTotalPaidFineAsync(1)).ReturnsAsync(50m);

        // Act
        var result = await _fineService.GetFineSummaryAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.MemberName.Should().Be("Test User");
        result.UnpaidFine.Should().Be(100m);
        result.TotalPaid.Should().Be(50m);
        result.TotalFine.Should().Be(150m);
    }

    [Fact]
    public async Task GetFineSummaryAsync_MemberNotFound_ShouldThrow()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Member?)null);

        // Act
        var act = async () => await _fineService.GetFineSummaryAsync(99);

        // Assert
        await act.Should().ThrowAsync<MemberNotFoundException>()
                 .WithMessage("*99*");
    }

    // ── PayFineAsync Tests ────────────────────────────────────
    [Fact]
    public async Task PayFineAsync_ValidPayment_ShouldSucceed()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(200m);
        _fineRepoMock.Setup(r => r.AddPaymentAsync(It.IsAny<FinePayment>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _fineService.PayFineAsync(1, 200m);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("₹200");
        _fineRepoMock.Verify(r => r.AddPaymentAsync(It.IsAny<FinePayment>()), Times.Once);
    }

    [Fact]
    public async Task PayFineAsync_NoPendingFine_ShouldThrow()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(0m);

        // Act
        var act = async () => await _fineService.PayFineAsync(1, 100m);

        // Assert
        await act.Should().ThrowAsync<LibraryException>()
                 .WithMessage("*No pending fines*");
    }

    [Fact]
    public async Task PayFineAsync_AmountExceedsUnpaidFine_ShouldThrow()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.GetTotalUnpaidFineAsync(1)).ReturnsAsync(100m);

        // Act
        var act = async () => await _fineService.PayFineAsync(1, 200m);

        // Assert
        await act.Should().ThrowAsync<LibraryException>()
                 .WithMessage("*exceeds*");
    }

    [Fact]
    public async Task PayFineAsync_MemberNotFound_ShouldThrow()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Member?)null);

        // Act
        var act = async () => await _fineService.PayFineAsync(99, 100m);

        // Assert
        await act.Should().ThrowAsync<MemberNotFoundException>()
                 .WithMessage("*99*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task PayFineAsync_InvalidAmount_ShouldThrow(decimal amount)
    {
        // Act
        var act = async () => await _fineService.PayFineAsync(1, amount);

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>()
                 .WithMessage("*greater than zero*");
    }

    // ── GetFineHistoryAsync Tests ─────────────────────────────
    [Fact]
    public async Task GetFineHistoryAsync_MemberWithHistory_ShouldReturnList()
    {
        // Arrange
        var payments = new List<FinePayment>
        {
            new FinePayment
            {
                Id          = 1,
                MemberId    = 1,
                BorrowId    = 1,
                AmountPaid  = 100m,
                PaymentDate = DateTime.Now,
                Borrow      = new DataAccessLayer.Models.Borrow
                {
                    BookCopy = new DataAccessLayer.Models.BookCopy
                    {
                        Book = new DataAccessLayer.Models.Book
                        {
                            Title = "Test Book"
                        }
                    }
                }
            }
        };

        _fineRepoMock.Setup(r => r.GetByMemberIdAsync(1)).ReturnsAsync(payments);

        // Act
        var result = await _fineService.GetFineHistoryAsync(1);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].AmountPaid.Should().Be(100m);
        result[0].BookTitle.Should().Be("Test Book");
    }

    [Fact]
    public async Task GetFineHistoryAsync_MemberWithNoHistory_ShouldReturnEmptyList()
    {
        // Arrange
        _fineRepoMock.Setup(r => r.GetByMemberIdAsync(1))
                     .ReturnsAsync(new List<FinePayment>());

        // Act
        var result = await _fineService.GetFineHistoryAsync(1);

        // Assert
        result.Should().BeEmpty();
    }

    // ── PayFineForBorrowAsync Tests ───────────────────────────
    [Fact]
    public async Task PayFineForBorrowAsync_ValidPayment_ShouldSucceed()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _fineRepoMock.Setup(r => r.AddPaymentAsync(It.IsAny<FinePayment>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _fineService.PayFineForBorrowAsync(1, 1, 100m);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("₹100");
        _fineRepoMock.Verify(r => r.AddPaymentAsync(It.IsAny<FinePayment>()), Times.Once);
    }

    [Fact]
    public async Task PayFineForBorrowAsync_MemberNotFound_ShouldThrow()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Member?)null);

        // Act
        var act = async () => await _fineService.PayFineForBorrowAsync(99, 1, 100m);

        // Assert
        await act.Should().ThrowAsync<MemberNotFoundException>();
    }

    [Theory]
    [InlineData(0, 1, 100)]
    [InlineData(1, 0, 100)]
    [InlineData(1, 1, 0)]
    public async Task PayFineForBorrowAsync_InvalidInputs_ShouldThrow(
        int memberId, int borrowId, decimal amount)
    {
        // Act
        var act = async () => await _fineService.PayFineForBorrowAsync(memberId, borrowId, amount);

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>();
    }
}