FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ChatbotService/CapFinLoan.Chatbot.API/CapFinLoan.Chatbot.API.csproj ChatbotService/CapFinLoan.Chatbot.API/
COPY ChatbotService/CapFinLoan.Chatbot.Application/CapFinLoan.Chatbot.Application.csproj ChatbotService/CapFinLoan.Chatbot.Application/
COPY ChatbotService/CapFinLoan.Chatbot.Infrastructure/CapFinLoan.Chatbot.Infrastructure.csproj ChatbotService/CapFinLoan.Chatbot.Infrastructure/
RUN dotnet restore "ChatbotService/CapFinLoan.Chatbot.API/CapFinLoan.Chatbot.API.csproj"
COPY ChatbotService/ ChatbotService/
WORKDIR /src/ChatbotService
RUN dotnet publish "CapFinLoan.Chatbot.API/CapFinLoan.Chatbot.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Chatbot.API.dll"]
