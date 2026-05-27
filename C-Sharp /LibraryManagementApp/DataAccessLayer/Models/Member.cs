using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Member
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int MembershipTypeId { get; set; }

    public int Status { get; set; }

    public DateOnly JoinedDate { get; set; }

    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

    public virtual ICollection<FinePayment> FinePayments { get; set; } = new List<FinePayment>();

    public virtual MembershipType MembershipType { get; set; } = null!;
}
