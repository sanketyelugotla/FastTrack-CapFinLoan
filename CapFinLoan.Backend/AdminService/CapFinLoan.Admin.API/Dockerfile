FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shared ./Shared
COPY AdminService ./AdminService
WORKDIR /src/AdminService
RUN dotnet restore "CapFinLoan.Admin.API/CapFinLoan.Admin.API.csproj"
RUN dotnet publish "CapFinLoan.Admin.API/CapFinLoan.Admin.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Admin.API.dll"]
