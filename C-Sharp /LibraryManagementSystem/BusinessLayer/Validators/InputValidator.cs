using BusinessLayer.Exceptions;
using System.Text.RegularExpressions;

namespace BusinessLayer.Validators;

public static class InputValidator
{
    public static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidInputException("Name", "Name cannot be empty.");

        if (name.Trim().Length < 2)
            throw new InvalidInputException("Name", "Name must be at least 2 characters.");

        if (name.Trim().Length > 150)
            throw new InvalidInputException("Name", "Name cannot exceed 150 characters.");

        if (!Regex.IsMatch(name.Trim(), @"^[a-zA-Z\s]+$"))
            throw new InvalidInputException("Name", "Name can only contain letters and spaces.");
    }

    public static void ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new InvalidInputException("Phone", "Phone number cannot be empty.");

        if (!Regex.IsMatch(phone.Trim(), @"^\d{10}$"))
            throw new InvalidInputException("Phone", "Phone number must be exactly 10 digits.");
    }

    public static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidInputException("Email", "Email cannot be empty.");

        if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new InvalidInputException("Email", "Email format is invalid. Example: user@example.com");

        if (email.Trim().Length > 150)
            throw new InvalidInputException("Email", "Email cannot exceed 150 characters.");
    }

    public static void ValidateIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new InvalidInputException("ISBN", "ISBN cannot be empty.");

        if (isbn.Trim().Length > 20)
            throw new InvalidInputException("ISBN", "ISBN cannot exceed 20 characters.");
    }

    public static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidInputException("Title", "Book title cannot be empty.");

        if (title.Trim().Length > 255)
            throw new InvalidInputException("Title", "Title cannot exceed 255 characters.");
    }

    public static void ValidateAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new InvalidInputException("Author", "Author name cannot be empty.");

        if (author.Trim().Length > 150)
            throw new InvalidInputException("Author", "Author name cannot exceed 150 characters.");
    }

    public static void ValidateCategoryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidInputException("Category", "Category name cannot be empty.");

        if (name.Trim().Length > 100)
            throw new InvalidInputException("Category", "Category name cannot exceed 100 characters.");
    }

    public static void ValidateId(int id, string fieldName = "ID")
    {
        if (id <= 0)
            throw new InvalidInputException(fieldName, $"{fieldName} must be a positive number.");
    }

    public static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidInputException("Amount", "Payment amount must be greater than zero.");
    }
}