using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Table("membership_type")]
[Index("Type", Name = "membership_type_type_key", IsUnique = true)]
public partial class MembershipType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("type")]
    public int Type { get; set; }

    [Column("max_borrowings")]
    public int MaxBorrowings { get; set; }

    [Column("max_borrow_days")]
    public int MaxBorrowDays { get; set; }

    [InverseProperty("MembershipType")]
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
