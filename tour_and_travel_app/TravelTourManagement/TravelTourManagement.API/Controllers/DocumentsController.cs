using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Ensure only authenticated users can access the proxy
public class DocumentsController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;

    public DocumentsController(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    [HttpGet("proxy")]
    public async Task<IActionResult> GetDocumentProxy([FromQuery] string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL parameter is required.");
        }

        try
        {
            var (content, contentType) = await _blobStorageService.DownloadFileAsync(url, cancellationToken);
            return File(content, contentType);
        }
        catch (System.Exception ex)
        {
            return NotFound($"Document not found or access denied. Details: {ex.Message}");
        }
    }
}
