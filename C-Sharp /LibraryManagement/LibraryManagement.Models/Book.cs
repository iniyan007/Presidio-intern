using System;
using System.Collections.Generic;

namespace LibraryManagement.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Isbn { get; set; } = null!;

    public int PublishedYear { get; set; }

    public int AvailableCopies { get; set; }
}
