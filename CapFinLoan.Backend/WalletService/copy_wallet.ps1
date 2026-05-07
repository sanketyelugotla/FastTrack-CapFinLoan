$sourceBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\ApplicationService\CapFinLoan.Application.Application"
$destBase = "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService\CapFinLoan.Wallet.Application"

New-Item -ItemType Directory -Force -Path "$destBase\Interfaces"
New-Item -ItemType Directory -Force -Path "$destBase\Services"
New-Item -ItemType Directory -Force -Path "$destBase\Exceptions"
New-Item -ItemType Directory -Force -Path "$destBase\Options"
New-Item -ItemType Directory -Force -Path "$destBase\Contracts\Requests"
New-Item -ItemType Directory -Force -Path "$destBase\Contracts\Responses"

# Copy Interfaces
Copy-Item "$sourceBase\Interfaces\IWalletRepository.cs" -Destination "$destBase\Interfaces\"
Copy-Item "$sourceBase\Interfaces\IWalletService.cs" -Destination "$destBase\Interfaces\"
Copy-Item "$sourceBase\Interfaces\IRazorpayGateway.cs" -Destination "$destBase\Interfaces\"

# Copy Services
Copy-Item "$sourceBase\Services\WalletService.cs" -Destination "$destBase\Services\"

# Copy Exceptions
Copy-Item "$sourceBase\Exceptions\InsufficientWalletBalanceException.cs" -Destination "$destBase\Exceptions\"
# Create a base WalletServiceException
@"
using System.Net;

namespace CapFinLoan.Wallet.Application.Exceptions;

public class WalletServiceException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }

    public WalletServiceException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string errorCode = "INTERNAL_ERROR") 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
"@ | Out-File -FilePath "$destBase\Exceptions\WalletServiceException.cs" -Encoding UTF8

# Copy Options
Copy-Item "$sourceBase\Options\RazorpayOptions.cs" -Destination "$destBase\Options\"
Copy-Item "$sourceBase\Options\WalletOptions.cs" -Destination "$destBase\Options\"

# Copy Contracts
Copy-Item "$sourceBase\Contracts\Requests\*Wallet*" -Destination "$destBase\Contracts\Requests\"
Copy-Item "$sourceBase\Contracts\Requests\*Razorpay*" -Destination "$destBase\Contracts\Requests\"
Copy-Item "$sourceBase\Contracts\Responses\*Wallet*" -Destination "$destBase\Contracts\Responses\"

# Replace Namespaces in all .cs files in Wallet.Application and Wallet.Domain
$files = Get-ChildItem -Path "f:\Capgemini Classes\CapFinLoan\CapFinLoan.Backend\WalletService" -Filter *.cs -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $content = $content -replace 'namespace CapFinLoan\.Application', 'namespace CapFinLoan.Wallet'
    $content = $content -replace 'using CapFinLoan\.Application', 'using CapFinLoan.Wallet'
    $content = $content -replace 'ApplicationServiceException', 'WalletServiceException'
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8
}

Write-Output "Done copying and replacing namespaces."
