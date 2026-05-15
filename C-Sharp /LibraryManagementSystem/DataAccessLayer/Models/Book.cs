using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Table("book")]
[Index("Isbn", Name = "book_isbn_key", IsUnique = true)]
public partial class Book
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("isbn")]
    [StringLength(20)]
    public string Isbn { get; set; } = null!;

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("author")]
    [StringLength(150)]
    public string Author { get; set; } = null!;

    [Column("category_id")]
    public int CategoryId { get; set; }

    [InverseProperty("Book")]
    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Books")]
    public virtual Category Category { get; set; } = null!;
}
