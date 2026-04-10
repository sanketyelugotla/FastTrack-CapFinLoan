FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy only csproj and solution files for restore layer
COPY NotificationService/CapFinLoan.Notification.API/CapFinLoan.Notification.API.csproj NotificationService/CapFinLoan.Notification.API/
COPY NotificationService/CapFinLoan.Notification.Application/CapFinLoan.Notification.Application.csproj NotificationService/CapFinLoan.Notification.Application/
COPY NotificationService/CapFinLoan.Notification.Domain/CapFinLoan.Notification.Domain.csproj NotificationService/CapFinLoan.Notification.Domain/
COPY NotificationService/CapFinLoan.Notification.Infrastructure/CapFinLoan.Notification.Infrastructure.csproj NotificationService/CapFinLoan.Notification.Infrastructure/
COPY NotificationService/CapFinLoan.Notification.Persistence/CapFinLoan.Notification.Persistence.csproj NotificationService/CapFinLoan.Notification.Persistence/
COPY Shared/ Shared/
RUN dotnet restore "NotificationService/CapFinLoan.Notification.API/CapFinLoan.Notification.API.csproj"
# Copy the rest of the source code
COPY NotificationService/ NotificationService/
WORKDIR /src/NotificationService
RUN dotnet publish "CapFinLoan.Notification.API/CapFinLoan.Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Notification.API.dll"]
