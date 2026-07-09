using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Ai;

namespace TravelTourManagement.Business.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    private const string SystemPrompt = @"You are the TourMate Travel Concierge, an AI assistant for a premium tour and travel booking platform called 'TourMate'. 
Your primary goal is to help users plan trips, provide travel advice, and answer questions about destinations, budgets, and itineraries in a friendly, enthusiastic, and highly professional tone. 
Keep your responses concise and helpful. Use emojis occasionally to make it engaging. 
IMPORTANT: Do NOT use any Markdown formatting (no asterisks for bold, no hash symbols for headers). Provide plain text only, separated by newlines.";

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiApiKey"] ?? string.Empty;
    }

    public async Task<ChatResponseDto> GenerateChatResponseAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_API_KEY_HERE")
        {
            return new ChatResponseDto { Reply = "AI is currently unavailable (Missing API Key)." };
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = SystemPrompt } }
            },
            contents = request.Messages.Select(m => new
            {
                role = m.Role.ToLower() == "ai" || m.Role.ToLower() == "assistant" || m.Role.ToLower() == "model" ? "model" : "user",
                parts = new[] { new { text = m.Text } }
            }).ToArray()
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload, jsonOptions), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ChatResponseDto { Reply = $"Error from API: {response.StatusCode} - {errorText}" };
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        using var jsonDoc = JsonDocument.Parse(responseString);
        
        try
        {
            var candidates = jsonDoc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() > 0)
            {
                var content = candidates[0].GetProperty("content");
                var parts = content.GetProperty("parts");
                if (parts.GetArrayLength() > 0)
                {
                    return new ChatResponseDto
                    {
                        Reply = parts[0].GetProperty("text").GetString() ?? "I'm sorry, I don't know what to say."
                    };
                }
            }
        }
        catch
        {
            // Fallback if parsing fails
        }

        return new ChatResponseDto { Reply = "Sorry, I couldn't process that response." };
    }
}
