using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Ai;

public class ChatMessageDto
{
    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;
}

public class ChatRequestDto
{
    [Required]
    public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
}

public class ChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
}
