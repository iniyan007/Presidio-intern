namespace LibraryManagement.Models.DTOs;

public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Isbn { get; set; } = string.Empty;

    public int PublishedYear { get; set; }

    public int AvailableCopies { get; set; }
}