FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only csproj files for restore layer
COPY WalletService/CapFinLoan.Wallet.Api/CapFinLoan.Wallet.Api.csproj WalletService/CapFinLoan.Wallet.Api/
COPY WalletService/CapFinLoan.Wallet.Application/CapFinLoan.Wallet.Application.csproj WalletService/CapFinLoan.Wallet.Application/
COPY WalletService/CapFinLoan.Wallet.Domain/CapFinLoan.Wallet.Domain.csproj WalletService/CapFinLoan.Wallet.Domain/
COPY WalletService/CapFinLoan.Wallet.Infrastructure/CapFinLoan.Wallet.Infrastructure.csproj WalletService/CapFinLoan.Wallet.Infrastructure/
COPY Shared/ Shared/

RUN dotnet restore "WalletService/CapFinLoan.Wallet.Api/CapFinLoan.Wallet.Api.csproj"

# Copy the rest of the source code
COPY WalletService/ WalletService/
WORKDIR /src/WalletService

RUN dotnet publish "CapFinLoan.Wallet.Api/CapFinLoan.Wallet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Wallet.Api.dll"]
