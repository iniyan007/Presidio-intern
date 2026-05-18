using DataAccessLayer.Models;

namespace Tests.Helpers;

public static class TestDataHelper
{
    public static Member CreateMember(
        int id = 1,
        string name = "Test User",
        string phone = "9876543210",
        string email = "test@email.com",
        int membershipTypeId = 1,
        int status = 0)
    {
        return new Member
        {
            Id               = id,
            Name             = name,
            Phone            = phone,
            Email            = email,
            MembershipTypeId = membershipTypeId,
            Status           = status,
            JoinedDate       = DateOnly.FromDateTime(DateTime.Today),
            MembershipType   = CreateMembershipType(membershipTypeId)
        };
    }

    public static MembershipType CreateMembershipType(int id = 1)
    {
        return id switch
        {
            1 => new MembershipType { Id = 1, Type = 0, MaxBorrowings = 2, MaxBorrowDays = 7  },  // Basic
            2 => new MembershipType { Id = 2, Type = 1, MaxBorrowings = 3, MaxBorrowDays = 10 },  // Student
            3 => new MembershipType { Id = 3, Type = 2, MaxBorrowings = 5, MaxBorrowDays = 15 },  // Premium
            _ => new MembershipType { Id = 1, Type = 0, MaxBorrowings = 2, MaxBorrowDays = 7  }
        };
    }

    public static Book CreateBook(
        int id = 1,
        string isbn = "978-0-06-112008-4",
        string title = "Test Book",
        string author = "Test Author",
        int categoryId = 1)
    {
        return new Book
        {
            Id         = id,
            Isbn       = isbn,
            Title      = title,
            Author     = author,
            CategoryId = categoryId,
            Category   = new Category { Id = categoryId, Name = "Fiction" }
        };
    }

    public static BookCopy CreateBookCopy(
        int id = 1,
        int bookId = 1,
        int status = 0,
        string? remarks = "Good condition")
    {
        return new BookCopy
        {
            Id      = id,
            BookId  = bookId,
            Status  = status,
            Remarks = remarks,
            Book    = CreateBook(bookId)
        };
    }

    public static Borrow CreateBorrow(
        int id = 1,
        int memberId = 1,
        int bookCopyId = 1,
        int status = 0,
        int dueDaysFromNow = 7)
    {
        return new Borrow
        {
            Id           = id,
            MemberId     = memberId,
            BookCopyId   = bookCopyId,
            DateOfBorrow = DateOnly.FromDateTime(DateTime.Today),
            DueDate      = DateOnly.FromDateTime(DateTime.Today.AddDays(dueDaysFromNow)),
            FineAmount   = 0,
            Status       = status,
            Member       = CreateMember(memberId),
            BookCopy     = CreateBookCopy(bookCopyId)
        };
    }

    public static Borrow CreateOverdueBorrow(
        int id = 1,
        int memberId = 1,
        int bookCopyId = 1,
        int overdueDays = 5)
    {
        return new Borrow
        {
            Id           = id,
            MemberId     = memberId,
            BookCopyId   = bookCopyId,
            DateOfBorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(-(overdueDays + 7))),
            DueDate      = DateOnly.FromDateTime(DateTime.Today.AddDays(-overdueDays)),
            FineAmount   = 0,
            Status       = 0,  // Still borrowed
            Member       = CreateMember(memberId),
            BookCopy     = CreateBookCopy(bookCopyId)
        };
    }
}