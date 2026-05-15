using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Table("fine_payments")]
public partial class FinePayment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("borrow_id")]
    public int BorrowId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("amount_paid")]
    [Precision(10, 2)]
    public decimal AmountPaid { get; set; }

    [Column("payment_date", TypeName = "timestamp without time zone")]
    public DateTime PaymentDate { get; set; }

    [ForeignKey("BorrowId")]
    [InverseProperty("FinePayments")]
    public virtual Borrow Borrow { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("FinePayments")]
    public virtual Member Member { get; set; } = null!;
}
