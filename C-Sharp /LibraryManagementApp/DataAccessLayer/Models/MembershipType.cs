using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class MembershipType
{
    public int Id { get; set; }

    public int Type { get; set; }

    public int MaxBorrowings { get; set; }

    public int MaxBorrowDays { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
