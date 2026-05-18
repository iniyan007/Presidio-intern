using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using FluentAssertions;
using Moq;
using Tests.Helpers;

namespace Tests.Services;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _memberRepoMock;
    private readonly Mock<IBookRepository>   _bookRepoMock;
    private readonly MemberService           _memberService;

    public MemberServiceTests()
    {
        _memberRepoMock = new Mock<IMemberRepository>();
        _bookRepoMock   = new Mock<IBookRepository>();
        _memberService  = new MemberService(_memberRepoMock.Object, _bookRepoMock.Object);
    }

    // ── Add Member Tests ──────────────────────────────────────
    [Fact]
    public async Task AddMemberAsync_ValidInput_ShouldSucceed()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);
        _memberRepoMock.Setup(r => r.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);
        _memberRepoMock.Setup(r => r.AddAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        // Act
        var (success, message, memberId) = await _memberService.AddMemberAsync(
            "John Doe", "9876543210", "john@email.com", MembershipTypeEnum.Basic);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("John Doe");
        _memberRepoMock.Verify(r => r.AddAsync(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_DuplicateEmail_ShouldThrow()
    {
        // Arrange
        var existingMember = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync(existingMember);

        // Act
        var act = async () => await _memberService.AddMemberAsync(
            "John Doe", "9876543210", "test@email.com", MembershipTypeEnum.Basic);

        // Assert
        await act.Should().ThrowAsync<LibraryException>()
                 .WithMessage("*email already exists*");
    }

    [Fact]
    public async Task AddMemberAsync_DuplicatePhone_ShouldThrow()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((Member?)null);
        _memberRepoMock.Setup(r => r.GetByPhoneAsync(It.IsAny<string>()))
                       .ReturnsAsync(TestDataHelper.CreateMember());

        // Act
        var act = async () => await _memberService.AddMemberAsync(
            "John Doe", "9876543210", "john@email.com", MembershipTypeEnum.Basic);

        // Assert
        await act.Should().ThrowAsync<LibraryException>()
                 .WithMessage("*phone number already exists*");
    }

    [Theory]
    [InlineData("", "9876543210", "john@email.com")]
    [InlineData("John", "123", "john@email.com")]
    [InlineData("John", "9876543210", "invalidemail")]
    public async Task AddMemberAsync_InvalidInput_ShouldThrow(
        string name, string phone, string email)
    {
        // Act
        var act = async () => await _memberService.AddMemberAsync(
            name, phone, email, MembershipTypeEnum.Basic);

        // Assert
        await act.Should().ThrowAsync<InvalidInputException>();
    }

    // ── Get Member Tests ──────────────────────────────────────
    [Fact]
    public async Task GetMemberByIdAsync_ExistingId_ShouldReturnMember()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        // Act
        var result = await _memberService.GetMemberByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task GetMemberByIdAsync_NonExistingId_ShouldReturnNull()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Member?)null);

        // Act
        var result = await _memberService.GetMemberByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    // ── Deactivate Member Tests ───────────────────────────────
    [Fact]
    public async Task DeactivateMemberAsync_ExistingMember_ShouldSucceed()
    {
        // Arrange
        var member = TestDataHelper.CreateMember();
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _memberRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _memberService.DeactivateMemberAsync(1);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("Inactive");
        _memberRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateMemberAsync_NonExistingMember_ShouldThrow()
    {
        // Arrange
        _memberRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Member?)null);

        // Act
        var act = async () => await _memberService.DeactivateMemberAsync(99);

        // Assert
        await act.Should().ThrowAsync<MemberNotFoundException>()
                 .WithMessage("*99*");
    }
}