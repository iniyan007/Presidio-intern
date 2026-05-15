namespace BusinessLayer.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Isbn { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}