FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy only csproj and solution files for restore layer
COPY ApplicationService/CapFinLoan.Application.API/CapFinLoan.Application.API.csproj ApplicationService/CapFinLoan.Application.API/
COPY ApplicationService/CapFinLoan.Application.Application/CapFinLoan.Application.Application.csproj ApplicationService/CapFinLoan.Application.Application/
COPY ApplicationService/CapFinLoan.Application.Domain/CapFinLoan.Application.Domain.csproj ApplicationService/CapFinLoan.Application.Domain/
COPY ApplicationService/CapFinLoan.Application.Infrastructure/CapFinLoan.Application.Infrastructure.csproj ApplicationService/CapFinLoan.Application.Infrastructure/
COPY ApplicationService/CapFinLoan.Application.Persistence/CapFinLoan.Application.Persistence.csproj ApplicationService/CapFinLoan.Application.Persistence/
COPY Shared/ Shared/
RUN dotnet restore "ApplicationService/CapFinLoan.Application.API/CapFinLoan.Application.API.csproj"
# Copy the rest of the source code
COPY ApplicationService/ ApplicationService/
WORKDIR /src/ApplicationService
RUN dotnet publish "CapFinLoan.Application.API/CapFinLoan.Application.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Application.API.dll"]
