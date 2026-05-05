using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStoreWeb.Services.PayOS
{
    public interface IPayOSService
    {
        Task<string?> CreatePaymentUrl(string orderId, decimal amount, string description, string returnUrl);
        bool VerifyWebhookSignature(string jsonBody, string signature);
    }

    public class PayOSService : IPayOSService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(IConfiguration configuration, HttpClient httpClient, ILogger<PayOSService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> CreatePaymentUrl(string orderId, decimal amount, string description, string returnUrl)
        {
            var clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID");
            var apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY");
            var checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY");
            var payOSReturnUrl = _configuration["PayOS:ReturnUrl"] ?? returnUrl;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
            {
                _logger.LogError("PayOS configuration is missing");
                return null;
            }

            // Tạo request body
            var orderCode = long.Parse(DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            var requestBody = new PayOSCreatePaymentRequest
            {
                OrderCode = orderCode,
                Amount = (int)amount,
                Description = description.Length > 25 ? description[..25] : description,
                ReturnUrl = payOSReturnUrl,
                CancelUrl = payOSReturnUrl,
                ExpiredAt = (int)(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds())
            };

            // Tạo signature
            var signatureData = $"amount={requestBody.Amount}&cancelUrl={requestBody.CancelUrl}&description={requestBody.Description}&orderCode={requestBody.OrderCode}&returnUrl={requestBody.ReturnUrl}";
            var signature = ComputeHmacSha256(signatureData, checksumKey);
            requestBody.Signature = signature;

            // Gọi API PayOS
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-client-id", clientId);
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("PayOS Request: {Request}", json);

                var response = await _httpClient.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("PayOS Response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PayOSCreatePaymentResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Data?.CheckoutUrl != null)
                    {
                        return result.Data.CheckoutUrl;
                    }
                }

                _logger.LogError("PayOS API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PayOS API");
                return null;
            }
        }

        public bool VerifyWebhookSignature(string jsonBody, string signature)
        {
            var checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY");
            if (string.IsNullOrEmpty(checksumKey) || string.IsNullOrEmpty(signature))
            {
                return false;
            }

            var computedSignature = ComputeHmacSha256(jsonBody, checksumKey);
            return computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }

        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public class PayOSCreatePaymentRequest
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("cancelUrl")]
        public string CancelUrl { get; set; } = string.Empty;

        [JsonPropertyName("returnUrl")]
        public string ReturnUrl { get; set; } = string.Empty;

        [JsonPropertyName("expiredAt")]
        public int ExpiredAt { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }
    }

    public class PayOSCreatePaymentResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public PayOSPaymentData? Data { get; set; }
    }

    public class PayOSPaymentData
    {
        [JsonPropertyName("bin")]
        public string? Bin { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("accountName")]
        public string? AccountName { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("paymentLinkId")]
        public string? PaymentLinkId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("checkoutUrl")]
        public string? CheckoutUrl { get; set; }

        [JsonPropertyName("qrCode")]
        public string? QrCode { get; set; }
    }

    public class PayOSWebhookData
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("transactionDateTime")]
        public string? TransactionDateTime { get; set; }

        [JsonPropertyName("paymentLinkId")]
        public string? PaymentLinkId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("desc")]
        public string? Desc { get; set; }

        [JsonPropertyName("counterAccountBankId")]
        public string? CounterAccountBankId { get; set; }

        [JsonPropertyName("counterAccountName")]
        public string? CounterAccountName { get; set; }

        [JsonPropertyName("counterAccountNumber")]
        public string? CounterAccountNumber { get; set; }

        [JsonPropertyName("refId")]
        public string? RefId { get; set; }

        [JsonPropertyName("paymentLinkId")]
        public string? PaymentLinkIdDuplicate { get; set; }
    }

    public class PayOSWebhookRequest
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public PayOSWebhookData? Data { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}
