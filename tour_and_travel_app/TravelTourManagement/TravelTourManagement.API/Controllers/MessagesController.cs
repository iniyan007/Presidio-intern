using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Messages;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    private bool IsPackager() => User.IsInRole("packager");

    [HttpPost("threads/init")]
    public async Task<IActionResult> GetOrInitializeThread([FromBody] CreateThreadRequest request, CancellationToken cancellationToken)
    {
        var thread = await _messageService.GetOrInitializeThreadAsync(GetUserId(), request.PackagerId, request.PackageId, cancellationToken);
        return Ok(thread);
    }

    [HttpGet("threads")]
    public async Task<IActionResult> GetUserThreads(CancellationToken cancellationToken)
    {
        var threads = await _messageService.GetUserThreadsAsync(GetUserId(), IsPackager(), cancellationToken);
        return Ok(threads);
    }

    [HttpGet("threads/{threadId}/messages")]
    public async Task<IActionResult> GetThreadMessages(Guid threadId, CancellationToken cancellationToken)
    {
        var messages = await _messageService.GetThreadMessagesAsync(threadId, GetUserId(), cancellationToken);
        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await _messageService.SendMessageAsync(GetUserId(), request, cancellationToken);
        return Ok(message);
    }

    [HttpPut("threads/{threadId}/read")]
    public async Task<IActionResult> MarkMessagesAsRead(Guid threadId, [FromQuery] MessageSenderRole readerRole, CancellationToken cancellationToken)
    {
        var success = await _messageService.MarkMessagesAsReadAsync(threadId, GetUserId(), readerRole, cancellationToken);
        if (!success) return BadRequest(new { message = "Failed to mark messages as read." });
        return Ok(new { success = true });
    }
}
