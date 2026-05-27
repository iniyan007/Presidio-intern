using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class FinePayment
{
    public int Id { get; set; }

    public int BorrowId { get; set; }

    public int MemberId { get; set; }

    public decimal AmountPaid { get; set; }

    public DateTime PaymentDate { get; set; }

    public virtual Borrow Borrow { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
