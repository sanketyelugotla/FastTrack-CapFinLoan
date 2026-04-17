using CapFinLoan.Chatbot.Application.Contracts.Requests;
using CapFinLoan.Chatbot.Application.Contracts.Responses;

namespace CapFinLoan.Chatbot.Application.Interfaces;

public interface IChatOrchestrationService
{
    Task<ChatMessageResponse> ProcessMessageAsync(
        ChatMessageRequest request,
        string userRole,
        string userId,
        string bearerToken,
        CancellationToken cancellationToken = default);
}
