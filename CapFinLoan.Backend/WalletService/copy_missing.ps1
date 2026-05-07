$sourceBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\ApplicationService\CapFinLoan.Application.Application"
$destBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService\CapFinLoan.Wallet.Application"

$sourceDomain = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\ApplicationService\CapFinLoan.Application.Domain"
$destDomain = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService\CapFinLoan.Wallet.Domain"

Copy-Item "$sourceBase\Contracts\Requests\CreateTopUpOrderRequest.cs" -Destination "$destBase\Contracts\Requests\"
Copy-Item "$sourceBase\Contracts\Requests\VerifyTopUpRequest.cs" -Destination "$destBase\Contracts\Requests\"
Copy-Item "$sourceBase\Contracts\Requests\WithdrawRequest.cs" -Destination "$destBase\Contracts\Requests\"

Copy-Item "$sourceBase\Contracts\Responses\CreateTopUpOrderResponse.cs" -Destination "$destBase\Contracts\Responses\"
Copy-Item "$sourceBase\Contracts\Responses\VerifyTopUpResponse.cs" -Destination "$destBase\Contracts\Responses\"

New-Item -ItemType Directory -Force -Path "$destDomain\Constants"
Copy-Item "$sourceDomain\Constants\WalletEntryTypes.cs" -Destination "$destDomain\Constants\"
Copy-Item "$sourceDomain\Constants\WalletEntryDirections.cs" -Destination "$destDomain\Constants\"
Copy-Item "$sourceDomain\Constants\WalletOwnerTypes.cs" -Destination "$destDomain\Constants\"
Copy-Item "$sourceDomain\Constants\RoleNames.cs" -Destination "$destDomain\Constants\"

$files = Get-ChildItem -Path "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService" -Filter *.cs -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $content = $content -replace 'namespace CapFinLoan\.Application', 'namespace CapFinLoan.Wallet'
    $content = $content -replace 'using CapFinLoan\.Application', 'using CapFinLoan.Wallet'
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8
}

Write-Output "Done copying missed contracts and constants, replacing namespaces."
