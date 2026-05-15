using DataAccessLayer.Enums;

namespace BusinessLayer.DTOs;

public class MemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public MembershipTypeEnum MembershipType { get; set; }
    public MemberStatus Status { get; set; }
    public DateOnly JoinedDate { get; set; }
    public int MaxBorrowings { get; set; }
    public int MaxBorrowDays { get; set; }
}