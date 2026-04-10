FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy only csproj and solution files for restore layer
COPY AuthService/CapFinLoan.Auth.API/CapFinLoan.Auth.API.csproj AuthService/CapFinLoan.Auth.API/
COPY AuthService/CapFinLoan.Auth.Application/CapFinLoan.Auth.Application.csproj AuthService/CapFinLoan.Auth.Application/
COPY AuthService/CapFinLoan.Auth.Domain/CapFinLoan.Auth.Domain.csproj AuthService/CapFinLoan.Auth.Domain/
COPY AuthService/CapFinLoan.Auth.Infrastructure/CapFinLoan.Auth.Infrastructure.csproj AuthService/CapFinLoan.Auth.Infrastructure/
COPY AuthService/CapFinLoan.Auth.Persistence/CapFinLoan.Auth.Persistence.csproj AuthService/CapFinLoan.Auth.Persistence/
COPY Shared/ Shared/
RUN dotnet restore "AuthService/CapFinLoan.Auth.API/CapFinLoan.Auth.API.csproj"
# Copy the rest of the source code
COPY AuthService/ AuthService/
WORKDIR /src/AuthService
RUN dotnet publish "CapFinLoan.Auth.API/CapFinLoan.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Auth.API.dll"]
