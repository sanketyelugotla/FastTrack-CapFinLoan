using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CapFinLoan.Chatbot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Chatbot.Infrastructure;

/// <summary>
/// This client acts as an adapter to call the Groq API (or any OpenAI-compatible API)
/// using the OpenAI Chat Completions payload format, mapping it from our Gemini interfaces.
/// </summary>
public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public GeminiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiClient> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey is not configured.");
        _model = configuration["Gemini:Model"] ?? "llama-3.3-70b-versatile";
        _logger = logger;
    }

    public async Task<GeminiResponse> GenerateContentAsync(GeminiRequest request, CancellationToken cancellationToken = default)
    {
        // Groq API endpoint
        var url = "https://api.groq.com/openai/v1/chat/completions";

        var payload = BuildOpenAiPayload(request, _model);
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        _logger.LogDebug("Groq request: {Request}", json.Length > 2000 ? json[..2000] + "..." : json);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq API error ({StatusCode}): {Response}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Groq API returned {response.StatusCode}: {responseBody}");
        }

        _logger.LogDebug("Groq response: {Response}", responseBody.Length > 2000 ? responseBody[..2000] + "..." : responseBody);

        return MapOpenAiResponse(responseBody);
    }

    private static object BuildOpenAiPayload(GeminiRequest request, string model)
    {
        var messages = new List<object>();

        if (request.SystemInstruction is not null && request.SystemInstruction.Parts.Count > 0)
        {
            messages.Add(new { role = "system", content = request.SystemInstruction.Parts[0].Text });
        }

        foreach (var c in request.Contents)
        {
            var fnResponses = c.Parts.Where(p => p.FunctionResponse is not null).ToList();
            if (fnResponses.Count > 0)
            {
                foreach (var r in fnResponses)
                {
                    messages.Add(new
                    {
                        role = "tool",
                        tool_call_id = "call_" + r.FunctionResponse!.Name,
                        content = JsonSerializer.Serialize(r.FunctionResponse.Response)
                    });
                }
                continue;
            }

            var fnCalls = c.Parts.Where(p => p.FunctionCall is not null).ToList();
            if (fnCalls.Count > 0)
            {
                messages.Add(new
                {
                    role = "assistant",
                    content = c.Parts.FirstOrDefault(p => p.Text is not null)?.Text,
                    tool_calls = fnCalls.Select(fc => new
                    {
                        id = "call_" + fc.FunctionCall!.Name,
                        type = "function",
                        function = new
                        {
                            name = fc.FunctionCall.Name,
                            arguments = JsonSerializer.Serialize(fc.FunctionCall.Args)
                        }
                    }).ToArray()
                });
                continue;
            }

            var text = string.Join("\n", c.Parts.Where(p => p.Text is not null).Select(p => p.Text));
            messages.Add(new { role = c.Role == "model" ? "assistant" : "user", content = text });
        }

        object? tools = null;
        if (request.Tools?.Count > 0)
        {
            tools = request.Tools.SelectMany(t => t.FunctionDeclarations).Select(fd => new
            {
                type = "function",
                function = new
                {
                    name = fd.Name,
                    description = fd.Description,
                    parameters = fd.Parameters is not null ? BuildOpenAiSchema(fd.Parameters) : null
                }
            }).ToList();
        }

        return new
        {
            model = model,
            messages = messages,
            tools = tools,
            temperature = request.GenerationConfig?.Temperature,
            // Fallback for extreme cases
            max_tokens = Math.Min(request.GenerationConfig?.MaxOutputTokens ?? 4096, 4096)
        };
    }

    private static object BuildOpenAiSchema(GeminiSchema schema)
    {
        var result = new Dictionary<string, object> { ["type"] = schema.Type.ToLowerInvariant() };

        if (schema.Properties?.Count > 0)
        {
            result["properties"] = schema.Properties.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var prop = new Dictionary<string, object> { ["type"] = kvp.Value.Type.ToLowerInvariant() };
                    if (kvp.Value.Description is not null) prop["description"] = kvp.Value.Description;
                    if (kvp.Value.Enum?.Count > 0) prop["enum"] = kvp.Value.Enum;
                    return (object)prop;
                });
        }

        if (schema.Required?.Count > 0) result["required"] = schema.Required;

        return result;
    }

    private static GeminiResponse MapOpenAiResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var choices = doc.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() == 0) return new GeminiResponse();

        var message = choices[0].GetProperty("message");
        var role = message.TryGetProperty("role", out var r) ? r.GetString() : "model";
        if (role == "assistant") role = "model";

        var parts = new List<GeminiPart>();

        if (message.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String)
        {
            var text = c.GetString();
            if (!string.IsNullOrEmpty(text)) parts.Add(new GeminiPart { Text = text });
        }

        if (message.TryGetProperty("tool_calls", out var tcs) && tcs.ValueKind == JsonValueKind.Array)
        {
            foreach (var tc in tcs.EnumerateArray())
            {
                if (tc.TryGetProperty("function", out var f))
                {
                    var name = f.GetProperty("name").GetString();
                    var argsStr = f.GetProperty("arguments").GetString();
                    var args = string.IsNullOrWhiteSpace(argsStr) ? (JsonElement?)null : JsonDocument.Parse(argsStr).RootElement;
                    
                    parts.Add(new GeminiPart
                    {
                        FunctionCall = new GeminiFunctionCall
                        {
                            Name = name ?? "",
                            Args = args
                        }
                    });
                }
            }
        }

        return new GeminiResponse
        {
            Candidates = new List<GeminiCandidate>
            {
                new GeminiCandidate
                {
                    Content = new GeminiContent
                    {
                        Role = role,
                        Parts = parts
                    }
                }
            }
        };
    }
}
