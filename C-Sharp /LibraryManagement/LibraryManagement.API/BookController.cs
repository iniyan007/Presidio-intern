using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Repository;
using LibraryManagement.Service;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models.DTOs;

namespace LibraryManagement.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        public readonly IBookService _bookService;
        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }
        [HttpGet]
        public ActionResult<List<Book>> GetAllBooks()
        {
            try
            {
                var books = _bookService.GetAllBooks();
                return Ok(books);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public ActionResult<Book> GetBookById(int id)
        {
            try
            {
                var book = _bookService.GetBookById(id);
                if (book == null)
                    return NotFound("No book with the given id - " + id);
                return Ok(book);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("search")]
        public ActionResult<Book> SearchBookByTitle(string title)
        {
            try
            {
                var book = _bookService.SearchBookByTitle(title);
                if (book == null)
                    return NotFound("No book with the given title - " + title);
                return Ok(book);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult AddBook(CreateBookRequest request)
        {
            try
            {
                var book = new Book
                {
                    Title = request.Title,
                    Author = request.Author,
                    Isbn = request.Isbn,
                    PublishedYear = request.PublishedYear,
                    AvailableCopies = request.AvailableCopies
                };
                _bookService.AddBook(book);
                return Created("", new
                {
                    message = "Book created successfully",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

