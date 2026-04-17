using System.Text.Json;

namespace CapFinLoan.Chatbot.Application.Interfaces;

public interface IGeminiClient
{
    Task<GeminiResponse> GenerateContentAsync(GeminiRequest request, CancellationToken cancellationToken = default);
}

// ─── Gemini API models ───────────────────────────────────────────────────────

public class GeminiRequest
{
    public SystemInstruction? SystemInstruction { get; set; }
    public List<GeminiContent> Contents { get; set; } = new();
    public List<GeminiTool>? Tools { get; set; }
    public GenerationConfig? GenerationConfig { get; set; }
}

public class SystemInstruction
{
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiContent
{
    public string Role { get; set; } = string.Empty;
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiPart
{
    public string? Text { get; set; }
    public GeminiFunctionCall? FunctionCall { get; set; }
    public GeminiFunctionResponse? FunctionResponse { get; set; }
}

public class GeminiFunctionCall
{
    public string Name { get; set; } = string.Empty;
    public JsonElement? Args { get; set; }
}

public class GeminiFunctionResponse
{
    public string Name { get; set; } = string.Empty;
    public JsonElement Response { get; set; }
}

public class GeminiTool
{
    public List<GeminiFunctionDeclaration> FunctionDeclarations { get; set; } = new();
}

public class GeminiFunctionDeclaration
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GeminiSchema? Parameters { get; set; }
}

public class GeminiSchema
{
    public string Type { get; set; } = "OBJECT";
    public Dictionary<string, GeminiPropertySchema>? Properties { get; set; }
    public List<string>? Required { get; set; }
}

public class GeminiPropertySchema
{
    public string Type { get; set; } = "STRING";
    public string? Description { get; set; }
    public List<string>? Enum { get; set; }
}

public class GenerationConfig
{
    public double? Temperature { get; set; }
    public int? MaxOutputTokens { get; set; }
}

public class GeminiResponse
{
    public List<GeminiCandidate>? Candidates { get; set; }
}

public class GeminiCandidate
{
    public GeminiContent? Content { get; set; }
}
