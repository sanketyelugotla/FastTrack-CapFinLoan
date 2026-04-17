using System.Text.Json;
using CapFinLoan.Chatbot.Application.Contracts.Requests;
using CapFinLoan.Chatbot.Application.Contracts.Responses;
using CapFinLoan.Chatbot.Application.FunctionCalling;
using CapFinLoan.Chatbot.Application.Interfaces;
using CapFinLoan.Chatbot.Application.Prompts;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Chatbot.Application.Services;

public class ChatOrchestrationService : IChatOrchestrationService
{
    private const int MaxFunctionCallRounds = 5;
    private const string RoleApplicant = "APPLICANT";

    private readonly IGeminiClient _geminiClient;
    private readonly IBackendApiClient _backendApiClient;
    private readonly ILogger<ChatOrchestrationService> _logger;

    public ChatOrchestrationService(
        IGeminiClient geminiClient,
        IBackendApiClient backendApiClient,
        ILogger<ChatOrchestrationService> logger)
    {
        _geminiClient = geminiClient;
        _backendApiClient = backendApiClient;
        _logger = logger;
    }

    public async Task<ChatMessageResponse> ProcessMessageAsync(
        ChatMessageRequest request,
        string userRole,
        string userId,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var isApplicant = string.Equals(userRole, RoleApplicant, StringComparison.OrdinalIgnoreCase);
        var systemPrompt = isApplicant ? ApplicantSystemPrompt.Text : AdminSystemPrompt.Text;
        var functionDeclarations = isApplicant
            ? ApplicantFunctions.GetDeclarations()
            : AdminFunctions.GetDeclarations();

        // Build conversation contents from history
        var contents = new List<GeminiContent>();
        foreach (var entry in request.ConversationHistory)
        {
            contents.Add(new GeminiContent
            {
                Role = entry.Role == "user" ? "user" : "model",
                Parts = new() { new GeminiPart { Text = entry.Content } }
            });
        }

        // Add the current user message
        contents.Add(new GeminiContent
        {
            Role = "user",
            Parts = new() { new GeminiPart { Text = request.Message } }
        });

        var geminiRequest = new GeminiRequest
        {
            SystemInstruction = new SystemInstruction
            {
                Parts = new() { new GeminiPart { Text = systemPrompt } }
            },
            Contents = contents,
            Tools = new()
            {
                new GeminiTool { FunctionDeclarations = functionDeclarations }
            },
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.7,
                MaxOutputTokens = 2048
            }
        };

        var actionsTaken = new List<ActionResult>();
        var executor = new FunctionExecutor(_backendApiClient);

        // Execute with function-calling loop
        for (var round = 0; round < MaxFunctionCallRounds; round++)
        {
            var response = await _geminiClient.GenerateContentAsync(geminiRequest, cancellationToken);
            var candidate = response.Candidates?.FirstOrDefault();
            var parts = candidate?.Content?.Parts;

            if (parts is null || parts.Count == 0)
            {
                return new ChatMessageResponse
                {
                    Reply = "I'm sorry, I wasn't able to process your request. Please try again.",
                    ActionsTaken = actionsTaken
                };
            }

            // Check if there are any function calls
            var functionCalls = parts.Where(p => p.FunctionCall is not null).ToList();

            if (functionCalls.Count == 0)
            {
                // No function calls — return the text response
                var textReply = string.Join("\n", parts.Where(p => p.Text is not null).Select(p => p.Text));
                var suggestions = GenerateSuggestions(isApplicant, textReply);

                return new ChatMessageResponse
                {
                    Reply = textReply,
                    ActionsTaken = actionsTaken,
                    Suggestions = suggestions
                };
            }

            // Execute function calls and add results back to conversation
            // Add the model's response with function calls to the conversation
            contents.Add(new GeminiContent
            {
                Role = "model",
                Parts = parts
            });

            foreach (var fc in functionCalls)
            {
                var call = fc.FunctionCall!;
                _logger.LogInformation("Executing function: {FunctionName}", call.Name);

                var (result, actionSummary) = await executor.ExecuteAsync(
                    call.Name, call.Args, userRole, bearerToken, cancellationToken);

                actionsTaken.Add(new ActionResult
                {
                    Action = call.Name,
                    Status = result.Contains("\"error\"") ? "failed" : "success",
                    Details = actionSummary
                });

                // Add function response to conversation
                var functionResponseJson = JsonSerializer.Deserialize<JsonElement>(result);
                contents.Add(new GeminiContent
                {
                    Role = "user",
                    Parts = new()
                    {
                        new GeminiPart
                        {
                            FunctionResponse = new GeminiFunctionResponse
                            {
                                Name = call.Name,
                                Response = functionResponseJson
                            }
                        }
                    }
                });
            }

            // Update the request for the next round
            geminiRequest.Contents = contents;
        }

        _logger.LogWarning("Exceeded max function call rounds ({MaxRounds})", MaxFunctionCallRounds);
        return new ChatMessageResponse
        {
            Reply = "I performed several actions but couldn't complete the full response. Here's a summary of what was done.",
            ActionsTaken = actionsTaken
        };
    }

    private static List<string> GenerateSuggestions(bool isApplicant, string reply)
    {
        if (isApplicant)
        {
            return new()
            {
                "What documents do I need?",
                "Show my applications",
                "Calculate my EMI",
                "Help me fill the form"
            };
        }

        return new()
        {
            "Show dashboard",
            "Show submitted applications",
            "Which applications need attention?",
            "Show pending documents"
        };
    }
}
