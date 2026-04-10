FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy only csproj and solution files for restore layer
COPY ApiGateway/CapFinLoan.Gateway.API/CapFinLoan.Gateway.API.csproj ApiGateway/CapFinLoan.Gateway.API/
COPY Shared/ Shared/
RUN dotnet restore "ApiGateway/CapFinLoan.Gateway.API/CapFinLoan.Gateway.API.csproj"
# Copy the rest of the source code
COPY ApiGateway/ ApiGateway/
WORKDIR /src/ApiGateway
RUN dotnet publish "CapFinLoan.Gateway.API/CapFinLoan.Gateway.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapFinLoan.Gateway.API.dll"]
