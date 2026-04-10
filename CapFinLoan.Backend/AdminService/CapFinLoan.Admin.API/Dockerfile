FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy only csproj and solution files for restore layer
COPY AdminService/CapFinLoan.Admin.API/CapFinLoan.Admin.API.csproj AdminService/CapFinLoan.Admin.API/
COPY AdminService/CapFinLoan.Admin.Application/CapFinLoan.Admin.Application.csproj AdminService/CapFinLoan.Admin.Application/
COPY AdminService/CapFinLoan.Admin.Domain/CapFinLoan.Admin.Domain.csproj AdminService/CapFinLoan.Admin.Domain/
COPY AdminService/CapFinLoan.Admin.Infrastructure/CapFinLoan.Admin.Infrastructure.csproj AdminService/CapFinLoan.Admin.Infrastructure/
COPY AdminService/CapFinLoan.Admin.Persistence/CapFinLoan.Admin.Persistence.csproj AdminService/CapFinLoan.Admin.Persistence/
COPY Shared/ Shared/
RUN dotnet restore "AdminService/CapFinLoan.Admin.API/CapFinLoan.Admin.API.csproj"
# Copy the rest of the source code
COPY AdminService/ AdminService/
WORKDIR /src/AdminService
RUN dotnet publish "CapFinLoan.Admin.API/CapFinLoan.Admin.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Admin.API.dll"]
