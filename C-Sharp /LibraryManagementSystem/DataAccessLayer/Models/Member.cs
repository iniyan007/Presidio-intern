using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Table("members")]
[Index("Email", Name = "members_email_key", IsUnique = true)]
[Index("Phone", Name = "members_phone_key", IsUnique = true)]
public partial class Member
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("phone")]
    [StringLength(15)]
    public string Phone { get; set; } = null!;

    [Column("email")]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Column("membership_type_id")]
    public int MembershipTypeId { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("joined_date")]
    public DateOnly JoinedDate { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

    [InverseProperty("Member")]
    public virtual ICollection<FinePayment> FinePayments { get; set; } = new List<FinePayment>();

    [ForeignKey("MembershipTypeId")]
    [InverseProperty("Members")]
    public virtual MembershipType MembershipType { get; set; } = null!;
}
