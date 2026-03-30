FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shared ./Shared
COPY ApplicationService ./ApplicationService
WORKDIR /src/ApplicationService
RUN dotnet restore "CapFinLoan.Application.API/CapFinLoan.Application.API.csproj"
RUN dotnet publish "CapFinLoan.Application.API/CapFinLoan.Application.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Application.API.dll"]
