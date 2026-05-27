using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Borrow
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public int BookCopyId { get; set; }

    public DateOnly DateOfBorrow { get; set; }

    public DateOnly DueDate { get; set; }

    public DateOnly? DateOfReturn { get; set; }

    public decimal FineAmount { get; set; }

    public int Status { get; set; }

    public virtual BookCopy BookCopy { get; set; } = null!;

    public virtual ICollection<FinePayment> FinePayments { get; set; } = new List<FinePayment>();

    public virtual Member Member { get; set; } = null!;
}
