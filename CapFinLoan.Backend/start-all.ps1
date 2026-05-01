# Start all CapFinLoan Microservices, API Gateway, and Python ChatbotService

Write-Host ""
Write-Host "╔══════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       CapFinLoan — Starting All Services     ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ── Paths ──────────────────────────────────────────────────────────────────
$basePath       = $PSScriptRoot
$rootPath       = Split-Path $basePath -Parent
$solutionPath   = Join-Path $basePath "CapFinLoan.Backend.slnx"
$chatbotPath    = Join-Path $rootPath "ChatbotService"

# ── .NET services ──────────────────────────────────────────────────────────
$services = @(
    @{ Name = "Auth Service";         Path = "AuthService\CapFinLoan.Auth.API";                Port = 5021 },
    @{ Name = "Application Service";  Path = "ApplicationService\CapFinLoan.Application.API";  Port = 5022 },
    @{ Name = "Document Service";     Path = "DocumentService\CapFinLoan.Document.API";         Port = 5023 },
    @{ Name = "Admin Service";        Path = "AdminService\CapFinLoan.Admin.API";               Port = 5024 },
    @{ Name = "Notification Service"; Path = "NotificationService\CapFinLoan.Notification.API"; Port = 5025 },
    @{ Name = "API Gateway";          Path = "ApiGateway\CapFinLoan.Gateway.API";               Port = 5020 }
)

# ── Step 1: Build .NET solution ────────────────────────────────────────────
Write-Host "► Building .NET solution..." -ForegroundColor Yellow
dotnet build "$solutionPath" --nologo -q

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed. Services were not started." -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "✓ Build successful." -ForegroundColor Green
Write-Host ""

# ── Step 2: Start .NET services ────────────────────────────────────────────
foreach ($service in $services) {
    Write-Host "  Starting $($service.Name)..." -ForegroundColor Yellow
    $fullPath = Join-Path $basePath $service.Path
    Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run --no-build --project `"$fullPath`""
}
Write-Host ""

# ── Step 3: Start Python ChatbotService ────────────────────────────────────
Write-Host "► Starting Python ChatbotService..." -ForegroundColor Yellow

$venvPython  = Join-Path $chatbotPath ".venv\Scripts\python.exe"
$venvUvicorn = Join-Path $chatbotPath ".venv\Scripts\uvicorn.exe"
$requirements = Join-Path $chatbotPath "requirements.txt"

# Create venv if it doesn't exist
if (-Not (Test-Path $venvPython)) {
    Write-Host "  No venv found — creating one..." -ForegroundColor DarkYellow
    python -m venv (Join-Path $chatbotPath ".venv")
    Write-Host "  Installing dependencies..." -ForegroundColor DarkYellow
    & (Join-Path $chatbotPath ".venv\Scripts\pip.exe") install -r $requirements -q
}

# Start uvicorn in a new window so it's easy to see logs separately
Start-Process -FilePath $venvUvicorn `
    -ArgumentList "app.main:app --host 0.0.0.0 --port 5026 --reload" `
    -WorkingDirectory $chatbotPath `
    -NoNewWindow

Write-Host "✓ ChatbotService started." -ForegroundColor Green
Write-Host ""

# ── Summary ────────────────────────────────────────────────────────────────
Write-Host "╔══════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║           All Services Running!              ║" -ForegroundColor Green
Write-Host "╠══════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "║  Auth Service       → http://localhost:5021  ║"
Write-Host "║  Application Svc    → http://localhost:5022  ║"
Write-Host "║  Document Service   → http://localhost:5023  ║"
Write-Host "║  Admin Service      → http://localhost:5024  ║"
Write-Host "║  Notification Svc   → http://localhost:5025  ║"
Write-Host "║  Chatbot Service    → http://localhost:5026  ║" -ForegroundColor Cyan
Write-Host "║  API Gateway        → http://localhost:5020  ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
