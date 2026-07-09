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

using TravelTourManagement.DataAccess.DTOs.Packages;
using System.Collections.Generic;

namespace TravelTourManagement.Business.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IPackageService _packageService;

    private const string SystemPrompt = @"You are the TourMate Travel Concierge, an AI assistant for a premium tour and travel booking platform called 'TourMate'. 
Your primary goal is to help users plan trips, provide travel advice, and answer questions about destinations, budgets, and itineraries in a friendly, enthusiastic, and highly professional tone. 
Keep your responses concise and helpful. Use emojis occasionally to make it engaging. 
IMPORTANT: Do NOT use any Markdown formatting (no asterisks for bold, no hash symbols for headers). Provide plain text only, separated by newlines.";

    public AiService(HttpClient httpClient, IConfiguration configuration, IPackageService packageService)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiApiKey"] ?? string.Empty;
        _packageService = packageService;
    }

    public async Task<ChatResponseDto> GenerateChatResponseAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return new ChatResponseDto { Reply = "AI is currently unavailable (Missing API Key)." };
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // 1. Initial Request with Tools
        var payload = CreatePayload(request.Messages, null, null);
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload, jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ChatResponseDto { Reply = "I'm having a little trouble connecting right now. Please try again later." };
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        using var jsonDoc = JsonDocument.Parse(responseString);
        var candidates = jsonDoc.RootElement.GetProperty("candidates");
        if (candidates.GetArrayLength() == 0) return new ChatResponseDto { Reply = "Sorry, I couldn't process that response." };
        
        var content = candidates[0].GetProperty("content");
        var parts = content.GetProperty("parts");
        if (parts.GetArrayLength() == 0) return new ChatResponseDto { Reply = "Sorry, I couldn't process that response." };

        var firstPart = parts[0];

        // Check if the AI wants to call a function
        if (firstPart.TryGetProperty("functionCall", out var functionCall))
        {
            var functionName = functionCall.GetProperty("name").GetString();
            object? searchResults = null;

            if (functionName == "search_packages")
            {
                var args = functionCall.GetProperty("args");
                var dest = args.TryGetProperty("destination", out var d) ? d.GetString() : null;
                var country = args.TryGetProperty("country", out var c) ? c.GetString() : null;
                var packageType = args.TryGetProperty("packageType", out var pt) ? pt.GetString() : null;
                var searchTerm = args.TryGetProperty("searchTerm", out var st) ? st.GetString() : null;
                var maxPrice = args.TryGetProperty("maxPrice", out var p) && p.ValueKind == JsonValueKind.Number ? p.GetDecimal() : (decimal?)null;

                // Execute local function
                var packages = await _packageService.SearchPackagesAsync(new PackageSearchRequest { 
                    Destination = dest, 
                    Country = country,
                    PackageType = packageType,
                    SearchTerm = searchTerm,
                    MaxPrice = maxPrice, 
                    PageSize = 5 
                }, cancellationToken);
                searchResults = packages.Items.Select(p => new { p.Id, p.Title, p.Destination, p.StartingPrice, p.DurationDays, p.PackageType }).ToList();
            }
            else if (functionName == "get_package_details")
            {
                var args = functionCall.GetProperty("args");
                var packageTitle = args.TryGetProperty("packageTitle", out var pt) ? pt.GetString() : null;
                
                if (!string.IsNullOrEmpty(packageTitle))
                {
                    try
                    {
                        // Search for the package by title to get its ID
                        var packages = await _packageService.SearchPackagesAsync(new PackageSearchRequest { SearchTerm = packageTitle, PageSize = 1 }, cancellationToken);
                        var packageId = packages.Items.FirstOrDefault()?.Id;

                        if (packageId != null && packageId != Guid.Empty)
                        {
                            var package = await _packageService.GetPublishedPackageByIdAsync(packageId.Value, cancellationToken);
                            
                            // We extract only the most important info to avoid token limits
                            searchResults = new 
                            {
                                package.Title,
                                package.Description,
                                package.Destination,
                                package.DurationDays,
                                package.CancellationPolicy,
                                Highlights = package.Highlights,
                                Inclusions = package.Inclusions,
                                Exclusions = package.Exclusions,
                                Itinerary = package.ItineraryDays?.Select(d => new { 
                                    d.DayNumber, 
                                    d.Title, 
                                    d.Description, 
                                    Accommodations = d.Accommodations?.Select(a => new { a.HotelName, a.RoomType, a.StarRating, a.CheckInTime, a.CheckOutTime }),
                                    Activities = d.Activities?.Select(a => new { a.ActivityTitle, a.Description, a.DurationMinutes }),
                                    Meals = d.Meals?.Select(m => new { m.Description, m.MealType, m.IsIncluded })
                                })
                            };
                        }
                        else
                        {
                            searchResults = new { Error = "Package not found. Please try another package name." };
                        }
                    }
                    catch
                    {
                        searchResults = new { Error = "An error occurred while retrieving package details." };
                    }
                }
                else
                {
                    searchResults = new { Error = "Invalid package title provided." };
                }
            }

            if (searchResults != null)
            {
                // 2. Send Second Request with Function Result
                var functionResponsePayload = CreatePayload(request.Messages, functionCall, searchResults);
                var jsonContent2 = new StringContent(JsonSerializer.Serialize(functionResponsePayload, jsonOptions), Encoding.UTF8, "application/json");
                var response2 = await _httpClient.PostAsync(url, jsonContent2, cancellationToken);

                if (response2.IsSuccessStatusCode)
                {
                    var responseString2 = await response2.Content.ReadAsStringAsync(cancellationToken);
                    using var jsonDoc2 = JsonDocument.Parse(responseString2);
                    var parts2 = jsonDoc2.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts");
                    if (parts2.GetArrayLength() > 0 && parts2[0].TryGetProperty("text", out var textProp2))
                    {
                        return new ChatResponseDto { Reply = textProp2.GetString() ?? "I found the details but couldn't format them." };
                    }
                }
            }
        }
        if (firstPart.TryGetProperty("text", out var textProp))
        {
            return new ChatResponseDto { Reply = textProp.GetString() ?? "I'm sorry, I don't know what to say." };
        }

        return new ChatResponseDto { Reply = "Sorry, I couldn't process that response." };
    }

    private object CreatePayload(IEnumerable<ChatMessageDto> chatHistory, JsonElement? functionCallToReturn = null, object? functionResponseData = null)
    {
        var contentsList = chatHistory.Select(m => new
        {
            role = m.Role.ToLower() == "ai" || m.Role.ToLower() == "assistant" || m.Role.ToLower() == "model" ? "model" : "user",
            parts = new[] { (object)new { text = m.Text } }
        }).ToList();

        if (functionCallToReturn.HasValue && functionResponseData != null)
        {
            contentsList.Add(new
            {
                role = "model",
                parts = new[] { (object)new { functionCall = functionCallToReturn.Value } }
            });
            contentsList.Add(new
            {
                role = "user",
                parts = new[] { (object)new { 
                    functionResponse = new {
                        name = functionCallToReturn.Value.GetProperty("name").GetString(),
                        response = new { name = "search_packages", content = functionResponseData }
                    }
                } }
            });
        }

        return new
        {
            systemInstruction = new { parts = new[] { new { text = SystemPrompt } } },
            contents = contentsList,
            tools = new[]
            {
                new
                {
                    functionDeclarations = new object[]
                    {
                        new
                        {
                            name = "search_packages",
                            description = "Search the live database for travel packages based on keywords, country, package type, destination, and max price. Call this when the user asks for trip suggestions, packages, or pricing.",
                            parameters = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    destination = new { type = "STRING", description = "The destination city or location" },
                                    country = new { type = "STRING", description = "The destination country" },
                                    packageType = new { type = "STRING", description = "The type or category of package (e.g. Honeymoon, Adventure, Family)" },
                                    searchTerm = new { type = "STRING", description = "General search keywords if the user gives a broad requirement" },
                                    maxPrice = new { type = "NUMBER", description = "The maximum price the user is willing to pay" }
                                }
                            }
                        },
                        new
                        {
                            name = "get_package_details",
                            description = "Fetch the full details of a specific package including the daily itinerary, activities, accommodation, and meals. Call this when the user asks for more details about a specific package. Pass the EXACT Title of the package you want details for.",
                            parameters = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    packageTitle = new { type = "STRING", description = "The exact title or name of the package" }
                                },
                                required = new[] { "packageTitle" }
                            }
                        }
                    }
                }
            }
        };
    }
}
