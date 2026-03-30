FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shared ./Shared
COPY AuthService ./AuthService
WORKDIR /src/AuthService
RUN dotnet restore "CapFinLoan.Auth.API/CapFinLoan.Auth.API.csproj"
RUN dotnet publish "CapFinLoan.Auth.API/CapFinLoan.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Auth.API.dll"]
