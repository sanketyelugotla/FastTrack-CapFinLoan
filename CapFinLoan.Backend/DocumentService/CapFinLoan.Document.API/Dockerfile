FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy only csproj and solution files for restore layer
COPY DocumentService/CapFinLoan.Document.API/CapFinLoan.Document.API.csproj DocumentService/CapFinLoan.Document.API/
COPY DocumentService/CapFinLoan.Document.Application/CapFinLoan.Document.Application.csproj DocumentService/CapFinLoan.Document.Application/
COPY DocumentService/CapFinLoan.Document.Domain/CapFinLoan.Document.Domain.csproj DocumentService/CapFinLoan.Document.Domain/
COPY DocumentService/CapFinLoan.Document.Infrastructure/CapFinLoan.Document.Infrastructure.csproj DocumentService/CapFinLoan.Document.Infrastructure/
COPY DocumentService/CapFinLoan.Document.Persistence/CapFinLoan.Document.Persistence.csproj DocumentService/CapFinLoan.Document.Persistence/
COPY Shared/ Shared/
RUN dotnet restore "DocumentService/CapFinLoan.Document.API/CapFinLoan.Document.API.csproj"
# Copy the rest of the source code
COPY DocumentService/ DocumentService/
WORKDIR /src/DocumentService
RUN dotnet publish "CapFinLoan.Document.API/CapFinLoan.Document.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Document.API.dll"]
