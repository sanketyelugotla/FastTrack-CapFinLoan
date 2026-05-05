# Stop all CapFinLoan Microservices, API Gateway, and Python ChatbotService

Write-Host ""
Write-Host "+================================================+" -ForegroundColor Cyan
Write-Host "|      CapFinLoan - Stopping All Services        |" -ForegroundColor Cyan
Write-Host "+================================================+" -ForegroundColor Cyan
Write-Host ""

# -- Stop .NET services --
$dotnetProcessNames = @(
    "CapFinLoan.Auth.API",
    "CapFinLoan.Application.API",
    "CapFinLoan.Admin.API",
    "CapFinLoan.Document.API",
    "CapFinLoan.Gateway.API",
    "CapFinLoan.Notification.API"
)

Write-Host ">> Stopping .NET services..." -ForegroundColor Yellow
foreach ($name in $dotnetProcessNames) {
    $procs = Get-Process | Where-Object { $_.ProcessName -like $name }
    foreach ($proc in $procs) {
        Write-Host "  Stopping $($proc.ProcessName) (PID: $($proc.Id))..." -ForegroundColor DarkYellow
        Stop-Process -Id $proc.Id -Force
    }
}
Write-Host "  OK: .NET services stopped." -ForegroundColor Green
Write-Host ""

# -- Stop Python ChatbotService (uvicorn) --
Write-Host ">> Stopping Python ChatbotService..." -ForegroundColor Yellow

$uvicornProcs = Get-Process | Where-Object { $_.ProcessName -in @("uvicorn", "python") } | Where-Object {
    try {
        $cmd = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
        $cmd -like "*app.main:app*" -or $cmd -like "*5026*"
    } catch { $false }
}

if ($uvicornProcs) {
    foreach ($proc in $uvicornProcs) {
        Write-Host "  Stopping $($proc.ProcessName) (PID: $($proc.Id))..." -ForegroundColor DarkYellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  OK: ChatbotService stopped." -ForegroundColor Green
} else {
    # Fallback: kill any process on port 5026
    $portProcs = Get-NetTCPConnection -LocalPort 5026 -ErrorAction SilentlyContinue |
                Select-Object -ExpandProperty OwningProcess -ErrorAction SilentlyContinue |
                Select-Object -Unique

    if ($portProcs) {
        foreach ($pid in $portProcs) {
            Write-Host "  Stopping process on port 5026 (PID: $pid)..." -ForegroundColor DarkYellow
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
        Write-Host "  OK: ChatbotService stopped." -ForegroundColor Green
    } else {
        Write-Host "  ChatbotService was not running." -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "+================================================+" -ForegroundColor Green
Write-Host "|        All services have been stopped.         |" -ForegroundColor Green
Write-Host "+================================================+" -ForegroundColor Green
Write-Host ""
