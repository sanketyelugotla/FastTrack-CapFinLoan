using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Application.Options;
using Microsoft.Extensions.Options;

namespace CapFinLoan.Application.Infrastructure.Payments;

public class RazorpayGateway : IRazorpayGateway
{
    private readonly HttpClient _httpClient;
    private readonly RazorpayOptions _options;

    public RazorpayGateway(HttpClient httpClient, IOptions<RazorpayOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");

        var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.KeyId}:{_options.KeySecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credential);
    }

    public async Task<string> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken = default)
    {
        var amountInPaise = decimal.ToInt64(decimal.Round(amount * 100, 0, MidpointRounding.AwayFromZero));

        var payload = new
        {
            amount = amountInPaise,
            currency,
            receipt,
            payment_capture = 1,
        };

        using var response = await _httpClient.PostAsJsonAsync("v1/orders", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!json.RootElement.TryGetProperty("id", out var idElement))
        {
            throw new InvalidOperationException("Razorpay order id missing in response.");
        }

        return idElement.GetString() ?? throw new InvalidOperationException("Razorpay order id is empty.");
    }

    public bool VerifySignature(string orderId, string paymentId, string signature)
    {
        if (string.IsNullOrWhiteSpace(_options.KeySecret))
        {
            return false;
        }

        var payload = $"{orderId}|{paymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.KeySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();
        return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
    }

    public string GetPublicKey()
    {
        return _options.KeyId;
    }
}
