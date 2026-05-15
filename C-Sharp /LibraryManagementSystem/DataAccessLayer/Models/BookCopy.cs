using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Table("book_copies")]
public partial class BookCopy
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("book_id")]
    public int BookId { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("remarks")]
    [StringLength(300)]
    public string? Remarks { get; set; }

    [ForeignKey("BookId")]
    [InverseProperty("BookCopies")]
    public virtual Book Book { get; set; } = null!;

    [InverseProperty("BookCopy")]
    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
}
