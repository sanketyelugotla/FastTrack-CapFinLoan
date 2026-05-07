$sourceBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\ApplicationService\CapFinLoan.Application.Application"
$destBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService\CapFinLoan.Wallet.Application"

# Copy missing exceptions
Copy-Item "$sourceBase\Exceptions\ApplicationValidationException.cs" -Destination "$destBase\Exceptions\WalletValidationException.cs"
Copy-Item "$sourceBase\Exceptions\ApplicationNotFoundException.cs" -Destination "$destBase\Exceptions\WalletNotFoundException.cs"
Copy-Item "$sourceBase\Exceptions\ApplicationConflictException.cs" -Destination "$destBase\Exceptions\WalletConflictException.cs"
Copy-Item "$sourceBase\Exceptions\ApplicationForbiddenException.cs" -Destination "$destBase\Exceptions\WalletForbiddenException.cs"

# Copy IEventPublisher.cs
Copy-Item "$sourceBase\Interfaces\IEventPublisher.cs" -Destination "$destBase\Interfaces\"

$files = Get-ChildItem -Path "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService" -Filter *.cs -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $content = $content -replace 'namespace CapFinLoan\.Application', 'namespace CapFinLoan.Wallet'
    $content = $content -replace 'using CapFinLoan\.Application', 'using CapFinLoan.Wallet'
    $content = $content -replace 'ApplicationValidationException', 'WalletValidationException'
    $content = $content -replace 'ApplicationNotFoundException', 'WalletNotFoundException'
    $content = $content -replace 'ApplicationConflictException', 'WalletConflictException'
    $content = $content -replace 'ApplicationForbiddenException', 'WalletForbiddenException'
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8
}

Write-Output "Done copying missed exceptions, event publisher, replacing namespaces."
