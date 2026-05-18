using BusinessLayer.Exceptions;
using BusinessLayer.Validators;
using FluentAssertions;

namespace Tests.Validators;

public class InputValidatorTests
{
    // ── Phone Tests ───────────────────────────────────────────
    [Fact]
    public void ValidatePhone_ValidPhone_ShouldNotThrow()
    {
        var act = () => InputValidator.ValidatePhone("9876543210");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidatePhone_EmptyPhone_ShouldThrow(string phone)
    {
        var act = () => InputValidator.ValidatePhone(phone);
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*Phone*cannot be empty*");
    }

    [Theory]
    [InlineData("12345")]           // Too short
    [InlineData("12345678901")]     // Too long
    [InlineData("abcdefghij")]      // Letters
    [InlineData("98765-43210")]     // Special chars
    public void ValidatePhone_InvalidFormat_ShouldThrow(string phone)
    {
        var act = () => InputValidator.ValidatePhone(phone);
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*10 digits*");
    }

    // ── Email Tests ───────────────────────────────────────────
    [Theory]
    [InlineData("test@email.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user123@test.in")]
    public void ValidateEmail_ValidEmail_ShouldNotThrow(string email)
    {
        var act = () => InputValidator.ValidateEmail(email);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateEmail_EmptyEmail_ShouldThrow(string email)
    {
        var act = () => InputValidator.ValidateEmail(email);
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*Email*cannot be empty*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@dotcom")]
    [InlineData("@nodomain.com")]
    [InlineData("noatsign.com")]
    public void ValidateEmail_InvalidFormat_ShouldThrow(string email)
    {
        var act = () => InputValidator.ValidateEmail(email);
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*invalid*");
    }

    // ── Name Tests ────────────────────────────────────────────
    [Fact]
    public void ValidateName_ValidName_ShouldNotThrow()
    {
        var act = () => InputValidator.ValidateName("John Doe");
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateName_EmptyName_ShouldThrow()
    {
        var act = () => InputValidator.ValidateName("");
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*Name*cannot be empty*");
    }

    [Fact]
    public void ValidateName_SingleChar_ShouldThrow()
    {
        var act = () => InputValidator.ValidateName("A");
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*at least 2 characters*");
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("John@Doe")]
    [InlineData("John_Doe")]
    public void ValidateName_WithNumbers_ShouldThrow(string name)
    {
        var act = () => InputValidator.ValidateName(name);
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*letters and spaces*");
    }

    // ── ID Tests ──────────────────────────────────────────────
    [Fact]
    public void ValidateId_ValidId_ShouldNotThrow()
    {
        var act = () => InputValidator.ValidateId(1);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ValidateId_InvalidId_ShouldThrow(int id)
    {
        var act = () => InputValidator.ValidateId(id);
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*positive number*");
    }

    // ── Amount Tests ──────────────────────────────────────────
    [Fact]
    public void ValidateAmount_ValidAmount_ShouldNotThrow()
    {
        var act = () => InputValidator.ValidateAmount(100m);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void ValidateAmount_InvalidAmount_ShouldThrow(decimal amount)
    {
        var act = () => InputValidator.ValidateAmount(amount);
        act.Should().Throw<InvalidInputException>()
           .WithMessage("*greater than zero*");
    }
}   