using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Table("borrow")]
public partial class Borrow
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("book_copy_id")]
    public int BookCopyId { get; set; }

    [Column("date_of_borrow")]
    public DateOnly DateOfBorrow { get; set; }

    [Column("due_date")]
    public DateOnly DueDate { get; set; }

    [Column("date_of_return")]
    public DateOnly? DateOfReturn { get; set; }

    [Column("fine_amount")]
    [Precision(10, 2)]
    public decimal FineAmount { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [ForeignKey("BookCopyId")]
    [InverseProperty("Borrows")]
    public virtual BookCopy BookCopy { get; set; } = null!;

    [InverseProperty("Borrow")]
    public virtual ICollection<FinePayment> FinePayments { get; set; } = new List<FinePayment>();

    [ForeignKey("MemberId")]
    [InverseProperty("Borrows")]
    public virtual Member Member { get; set; } = null!;
}
