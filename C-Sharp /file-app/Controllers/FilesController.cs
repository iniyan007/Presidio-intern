using Microsoft.AspNetCore.Mvc;

namespace ExcelApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _db;

    // Inject the database context via the constructor
    public FilesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Uploads an Excel file to the SQLite database.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var allowedExtensions = new[] { ".xls", ".xlsx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only Excel files (.xls, .xlsx) are allowed.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var fileRecord = new FileRecord
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Data = memoryStream.ToArray()
        };

        _db.Files.Add(fileRecord);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "File uploaded successfully", FileId = fileRecord.Id });
    }

    /// <summary>
    /// Downloads an Excel file from the SQLite database by ID.
    /// </summary>
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var fileRecord = await _db.Files.FindAsync(id);

        if (fileRecord is null)
        {
            return NotFound("File not found.");
        }

        return File(fileRecord.Data, fileRecord.ContentType, fileRecord.FileName);
    }
    /// <summary>
    /// Uploads multiple Excel files to the SQLite database in a single request.
    /// </summary>
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files)
    {
        // 1. Check if any files were sent
        if (files is null || files.Count == 0)
        {
            return BadRequest("No files were uploaded.");
        }

        var allowedExtensions = new[] { ".xls", ".xlsx" };
        var uploadResults = new List<object>(); // To track success/failure of each file
        var validFilesToSave = new List<FileRecord>();

        // 2. Process each file in the list
        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                uploadResults.Add(new { FileName = file.FileName, Status = "Failed", Reason = "File is empty." });
                continue;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                uploadResults.Add(new { FileName = file.FileName, Status = "Failed", Reason = "Invalid file type." });
                continue;
            }

            // Read valid file into a byte array
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            validFilesToSave.Add(new FileRecord
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Data = memoryStream.ToArray()
            });
        }

        // 3. Save all valid files to SQLite in a single batch
        if (validFilesToSave.Any())
        {
            _db.Files.AddRange(validFilesToSave);
            await _db.SaveChangesAsync();

            // Entity Framework automatically populates the 'Id' property after SaveChangesAsync
            foreach (var record in validFilesToSave)
            {
                uploadResults.Add(new { FileName = record.FileName, Status = "Success", FileId = record.Id });
            }
        }

        // 4. Return a summary report to the user
        return Ok(new { 
            Message = "Batch upload process completed.", 
            TotalProcessed = files.Count,
            TotalSaved = validFilesToSave.Count,
            Results = uploadResults 
        });
    }
}
