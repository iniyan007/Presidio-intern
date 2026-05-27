using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class BookCopy
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int Status { get; set; }

    public string? Remarks { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
}
