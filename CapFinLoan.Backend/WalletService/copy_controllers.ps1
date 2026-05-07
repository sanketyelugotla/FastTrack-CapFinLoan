$sourceBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\ApplicationService\CapFinLoan.Application.Api"
$destBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService\CapFinLoan.Wallet.Api"

New-Item -ItemType Directory -Force -Path "$destBase\Controllers"

# Copy Controllers
Copy-Item "$sourceBase\Controllers\WalletController.cs" -Destination "$destBase\Controllers\"
Copy-Item "$sourceBase\Controllers\AdminWalletController.cs" -Destination "$destBase\Controllers\"

# Replace Namespaces in all .cs files in Wallet.Api
$files = Get-ChildItem -Path "$destBase" -Filter *.cs -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $content = $content -replace 'namespace CapFinLoan\.Application', 'namespace CapFinLoan.Wallet'
    $content = $content -replace 'using CapFinLoan\.Application', 'using CapFinLoan.Wallet'
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8
}

Write-Output "Done copying and replacing namespaces for API layer."
