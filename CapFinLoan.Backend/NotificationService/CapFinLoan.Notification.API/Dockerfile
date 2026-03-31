FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shared ./Shared
COPY NotificationService ./NotificationService
WORKDIR /src/NotificationService
RUN dotnet restore "CapFinLoan.Notification.API/CapFinLoan.Notification.API.csproj"
RUN dotnet publish "CapFinLoan.Notification.API/CapFinLoan.Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Notification.API.dll"]
