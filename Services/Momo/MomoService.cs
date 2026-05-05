using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace BookStoreWeb.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponse> CreatePaymentAsync(string orderId, string orderInfo, decimal amount, string extraData = "");
    }

    public class MomoService : IMomoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MomoService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<MomoCreatePaymentResponse> CreatePaymentAsync(string orderId, string orderInfo, decimal amount, string extraData = "")
        {
            var momoConfig = _configuration.GetSection("MomoAPI");
            var partnerCode = momoConfig["PartnerCode"];
            var accessKey = momoConfig["AccessKey"];
            var secretKey = momoConfig["SecretKey"];
            var momoApiUrl = momoConfig["MomoApiUrl"];
            var returnUrl = momoConfig["ReturnUrl"];
            var notifyUrl = momoConfig["NotifyUrl"];

            var requestId = Guid.NewGuid().ToString();
            var requestType = "captureWallet";
            
            // Đảm bảo Amount là số nguyên KHÔNG có dấu phân cách (130000 chứ không phải 130.000)
            var amountValue = ((long)amount).ToString();
            
            // Tạo OrderId unique bằng cách thêm timestamp (tránh trùng khi gửi lại)
            var uniqueOrderId = $"{orderId}_{DateTime.Now:yyyyMMddHHmmss}";

            // Tạo raw signature theo đúng chuẩn MoMo v2
            // Thứ tự chuẩn: accessKey + amount + extraData + ipnUrl + orderId + orderInfo + partnerCode + redirectUrl + requestId + requestType
            var rawSignature = $"accessKey={accessKey}&amount={amountValue}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={uniqueOrderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";

            // Tạo chữ ký HMAC-SHA256
            var signature = ComputeHmacSha256(rawSignature, secretKey!);

            var requestBody = new MomoCreatePaymentRequest
            {
                PartnerCode = partnerCode,
                PartnerName = "BookStore",
                StoreId = "TestStore",
                RequestId = requestId,
                Amount = amountValue,
                OrderId = uniqueOrderId,
                OrderInfo = orderInfo,
                RedirectUrl = returnUrl,
                IpnUrl = notifyUrl,
                Lang = "vi",
                ExtraData = extraData,
                RequestType = requestType,
                Signature = signature
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(momoApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
            });

            return result!;
        }

        public bool ValidateSignature(string rawData, string signature, string secretKey)
        {
            var computedSignature = ComputeHmacSha256(rawData, secretKey);
            return computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }

    public class MomoCreatePaymentRequest
    {
        [JsonPropertyName("partnerCode")]
        public string? PartnerCode { get; set; }

        [JsonPropertyName("partnerName")]
        public string? PartnerName { get; set; }

        [JsonPropertyName("storeId")]
        public string? StoreId { get; set; }

        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("orderInfo")]
        public string? OrderInfo { get; set; }

        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }

        [JsonPropertyName("ipnUrl")]
        public string? IpnUrl { get; set; }

        [JsonPropertyName("extraData")]
        public string? ExtraData { get; set; }

        [JsonPropertyName("requestType")]
        public string? RequestType { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        [JsonPropertyName("lang")]
        public string? Lang { get; set; }
    }

    public class MomoCreatePaymentResponse
    {
        [JsonPropertyName("partnerCode")]
        public string? PartnerCode { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("responseTime")]
        public long ResponseTime { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("payUrl")]
        public string? PayUrl { get; set; }

        [JsonPropertyName("shortLink")]
        public string? ShortLink { get; set; }
    }

    public class MomoPaymentCallback
    {
        [JsonPropertyName("partnerCode")]
        public string? PartnerCode { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("orderInfo")]
        public string? OrderInfo { get; set; }

        [JsonPropertyName("orderType")]
        public string? OrderType { get; set; }

        [JsonPropertyName("transId")]
        public string? TransId { get; set; }

        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("payType")]
        public string? PayType { get; set; }

        [JsonPropertyName("responseTime")]
        public long ResponseTime { get; set; }

        [JsonPropertyName("extraData")]
        public string? ExtraData { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }
    }
}
