# Start all Microservices and the API Gateway in the background

Write-Host "Starting CapFinLoan Microservices..." -ForegroundColor Cyan

# Define paths
$basePath = $PSScriptRoot
$services = @(
    @{ Name = "Auth Service"; Path = "AuthService\CapFinLoan.Auth.API" },
    @{ Name = "Application Service"; Path = "ApplicationService\CapFinLoan.Application.API" },
    @{ Name = "Admin Service"; Path = "AdminService\CapFinLoan.Admin.API" },
    @{ Name = "Document Service"; Path = "DocumentService\CapFinLoan.Document.API" },
    @{ Name = "API Gateway"; Path = "ApiGateway\CapFinLoan.Gateway.API" }
)

foreach ($service in $services) {
    Write-Host "Starting $($service.Name)..." -ForegroundColor Yellow
    $fullPath = Join-Path $basePath $service.Path
    Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run --project `"$fullPath`""
}

Write-Host "All services started!" -ForegroundColor Green
Write-Host "Auth Service: http://localhost:5025"
Write-Host "Application Service: http://localhost:5256"
Write-Host "Document Service: http://localhost:5262"
Write-Host "Admin Service: http://localhost:5067"
Write-Host "API Gateway: http://localhost:5021" -ForegroundColor Magenta
