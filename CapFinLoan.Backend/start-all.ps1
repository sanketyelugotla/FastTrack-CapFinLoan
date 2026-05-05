# Start all CapFinLoan Microservices, API Gateway, and Python ChatbotService

Write-Host ""
Write-Host "+================================================+" -ForegroundColor Cyan
Write-Host "|      CapFinLoan - Starting All Services        |" -ForegroundColor Cyan
Write-Host "+================================================+" -ForegroundColor Cyan
Write-Host ""

# -- Paths --
$basePath     = $PSScriptRoot
$rootPath     = Split-Path $basePath -Parent
$chatbotPath  = Join-Path $rootPath "ChatbotService"
$envFile      = Join-Path $rootPath ".env"

# -- .NET service definitions (Name, project folder path, port) --
$services = @(
    @{ Name = "Auth Service";         Path = "AuthService\CapFinLoan.Auth.API";                Port = 5021 },
    @{ Name = "Application Service";  Path = "ApplicationService\CapFinLoan.Application.API";  Port = 5022 },
    @{ Name = "Document Service";     Path = "DocumentService\CapFinLoan.Document.API";         Port = 5023 },
    @{ Name = "Admin Service";        Path = "AdminService\CapFinLoan.Admin.API";               Port = 5024 },
    @{ Name = "Notification Service"; Path = "NotificationService\CapFinLoan.Notification.API"; Port = 5025 },
    @{ Name = "API Gateway";          Path = "ApiGateway\CapFinLoan.Gateway.API";               Port = 5020 }
)

# ============================================================
# Step 1: Load root .env into this PowerShell session
#   Child processes started by this script inherit these vars.
# ============================================================
Write-Host ">> Loading environment variables from .env..." -ForegroundColor Yellow
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        # Skip blank lines and comments
        if ($line -eq '' -or $line.StartsWith('#')) { return }
        $idx = $line.IndexOf('=')
        if ($idx -lt 1) { return }
        $key = $line.Substring(0, $idx).Trim()
        $val = $line.Substring($idx + 1).Trim().Trim('"').Trim("'")
        # Set in the current process so child processes inherit it
        [System.Environment]::SetEnvironmentVariable($key, $val, 'Process')
    }
    Write-Host "  OK: Environment variables loaded." -ForegroundColor Green
} else {
    Write-Host "  WARN: .env not found at $envFile — services may lack config." -ForegroundColor DarkYellow
}
Write-Host ""

# ============================================================
# Step 2: Clean stale obj cache files
#   Prevents MSB3492 errors when Docker previously had obj dirs open.
# ============================================================
Write-Host ">> Cleaning stale build cache..." -ForegroundColor Yellow
Get-ChildItem -Path $basePath -Recurse -Filter "*.cache" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -like "*\obj\*" } |
    ForEach-Object {
        try   { Remove-Item $_.FullName -Force -ErrorAction Stop }
        catch { Write-Host "  Skipped: $($_.Name)" -ForegroundColor DarkGray }
    }
Write-Host "  OK: Cache cleaned." -ForegroundColor Green
Write-Host ""

# ============================================================
# Step 3: Fix missing wwwroot for API Gateway
#   SwaggerForOcelotUI requires the wwwroot directory to exist.
# ============================================================
$gatewayWwwRoot = Join-Path $basePath "ApiGateway\CapFinLoan.Gateway.API\wwwroot"
if (-Not (Test-Path $gatewayWwwRoot)) {
    New-Item -ItemType Directory -Path $gatewayWwwRoot -Force | Out-Null
    Write-Host "  Created: ApiGateway\wwwroot" -ForegroundColor DarkGray
}

# ============================================================
# Step 4: Build shared Messaging.Contracts FIRST
#   All services depend on this — build it once before the rest.
# ============================================================
$contractsProj = Join-Path $basePath "Shared\CapFinLoan.Messaging.Contracts\CapFinLoan.Messaging.Contracts.csproj"
Write-Host ">> Building shared Messaging.Contracts..." -ForegroundColor Yellow
dotnet build "$contractsProj" --nologo -q
if ($LASTEXITCODE -ne 0) {
    Write-Host "  FAILED: Could not build Messaging.Contracts." -ForegroundColor Red
    exit 1
}
Write-Host "  OK: Messaging.Contracts built." -ForegroundColor Green
Write-Host ""

# ============================================================
# Step 5: Build each service project SEQUENTIALLY
#   Avoids parallel DLL copy lock errors (MSB3026/MSB3027).
# ============================================================
Write-Host ">> Building .NET services (sequential to avoid file locks)..." -ForegroundColor Yellow
foreach ($service in $services) {
    $projDir = Join-Path $basePath $service.Path
    # Find the .csproj inside the folder
    $csproj = Get-ChildItem -Path $projDir -Filter "*.csproj" | Select-Object -First 1
    if ($null -eq $csproj) {
        Write-Host "  WARN: No .csproj found in $($service.Path)" -ForegroundColor DarkYellow
        continue
    }
    Write-Host "  Building $($service.Name)..." -ForegroundColor DarkYellow
    dotnet build "$($csproj.FullName)" --nologo -q
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FAILED: $($service.Name) build failed." -ForegroundColor Red
        Write-Host "  Continuing with other services..." -ForegroundColor DarkGray
    }
}
Write-Host "  OK: All services built." -ForegroundColor Green
Write-Host ""

# ============================================================
# Step 6: Start all .NET services with --no-build
#   They are already built — just launch them.
# ============================================================
Write-Host ">> Launching .NET services..." -ForegroundColor Yellow
foreach ($service in $services) {
    Write-Host "  Starting $($service.Name) on port $($service.Port)..." -ForegroundColor Yellow
    $fullPath = Join-Path $basePath $service.Path
    Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run --no-build --project `"$fullPath`""
}
Write-Host "  OK: All .NET services launched." -ForegroundColor Green
Write-Host ""

# ============================================================
# Step 7: Start Python ChatbotService
# ============================================================
Write-Host ">> Starting Python ChatbotService..." -ForegroundColor Yellow

$venvPython  = Join-Path $chatbotPath ".venv\Scripts\python.exe"
$venvUvicorn = Join-Path $chatbotPath ".venv\Scripts\uvicorn.exe"
$requirements = Join-Path $chatbotPath "requirements.txt"

if (-Not (Test-Path $venvPython)) {
    Write-Host "  No venv found - creating one..." -ForegroundColor DarkYellow
    python -m venv (Join-Path $chatbotPath ".venv")
    Write-Host "  Installing dependencies..." -ForegroundColor DarkYellow
    & (Join-Path $chatbotPath ".venv\Scripts\pip.exe") install -r $requirements -q
}

if (Test-Path $venvUvicorn) {
    Start-Process -FilePath $venvUvicorn `
        -ArgumentList "app.main:app --host 0.0.0.0 --port 5026 --reload" `
        -WorkingDirectory $chatbotPath `
        -NoNewWindow
    Write-Host "  OK: ChatbotService started on port 5026." -ForegroundColor Green
} else {
    Write-Host "  WARN: uvicorn not found. Run: pip install -r requirements.txt inside ChatbotService." -ForegroundColor DarkYellow
}

Write-Host ""

# -- Summary --
Write-Host "+================================================+" -ForegroundColor Green
Write-Host "|    All Services Ready!                         |" -ForegroundColor Green
Write-Host "+------------------------------------------------+" -ForegroundColor Green
Write-Host "|  Auth Service      -> http://localhost:5021    |"
Write-Host "|  Application Svc  -> http://localhost:5022    |"
Write-Host "|  Document Service -> http://localhost:5023    |"
Write-Host "|  Admin Service    -> http://localhost:5024    |"
Write-Host "|  Notification Svc -> http://localhost:5025    |"
Write-Host "|  Chatbot Service  -> http://localhost:5026    |" -ForegroundColor Cyan
Write-Host "|  API Gateway      -> http://localhost:5020    |" -ForegroundColor Magenta
Write-Host "+================================================+" -ForegroundColor Green
Write-Host ""
Write-Host "NOTE: Services are now running. The app is ready to use." -ForegroundColor DarkYellow
Write-Host ""
