using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BookStoreWeb.Services.Vnpay
{
    public interface IVnpayService
    {
        string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string? ipAddress = null);
        bool ValidateSignature(string queryString, string vnp_SecureHash);
    }

    public class VnpayService : IVnpayService
    {
        private readonly IConfiguration _configuration;

        public VnpayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string? ipAddress = null)
        {
            var vnpayConfig = _configuration.GetSection("VNPAY");
            var tmnCode = vnpayConfig["TmnCode"];
            var hashSecret = vnpayConfig["HashSecret"];
            var baseUrl = vnpayConfig["BaseUrl"];
            var returnUrl = vnpayConfig["ReturnUrl"];
            var version = vnpayConfig["Version"];
            var command = vnpayConfig["Command"];
            var currCode = vnpayConfig["CurrCode"];
            var locale = vnpayConfig["Locale"];

            ipAddress ??= "127.0.0.1";

            // Tạo OrderId unique
            var uniqueOrderId = $"{orderId}_{DateTime.Now:yyyyMMddHHmmss}";
            
            // Chuyển amount sang đơn vị VNĐ (nhân 100 - VNPAY yêu cầu)
            var vnpAmount = (long)(amount * 100);

            // Tạo các tham số VNPAY
            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", version! },
                { "vnp_Command", command! },
                { "vnp_TmnCode", tmnCode! },
                { "vnp_Locale", locale! },
                { "vnp_CurrCode", currCode! },
                { "vnp_TxnRef", uniqueOrderId },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_Amount", vnpAmount.ToString() },
                { "vnp_ReturnUrl", returnUrl! },
                { "vnp_IpAddr", ipAddress },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            // Tạo chuỗi raw data cho checksum
            var rawData = BuildQueryString(vnpParams, false);
            Console.WriteLine($"[VNPAY] Raw data: {rawData}");

            // Tạo SecureHash (HMACSHA512)
            var secureHash = ComputeHmacSha512(rawData, hashSecret!);
            Console.WriteLine($"[VNPAY] SecureHash: {secureHash}");
            vnpParams.Add("vnp_SecureHash", secureHash);

            // Build URL với hash
            var paymentUrl = baseUrl + "?" + BuildQueryString(vnpParams, true);
            Console.WriteLine($"[VNPAY] Payment URL: {paymentUrl}");

            return paymentUrl;
        }

        public bool ValidateSignature(string queryString, string vnp_SecureHash)
        {
            var vnpayConfig = _configuration.GetSection("VNPAY");
            var hashSecret = vnpayConfig["HashSecret"];

            // Parse query string
            var parsed = HttpUtility.ParseQueryString(queryString);
            var vnpParams = new SortedDictionary<string, string>();

            foreach (var key in parsed.AllKeys)
            {
                if (key != null && !key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
                {
                    vnpParams.Add(key, parsed[key]!);
                }
            }

            var rawData = BuildQueryString(vnpParams, false);
            var computedHash = ComputeHmacSha512(rawData, hashSecret!);

            return computedHash.Equals(vnp_SecureHash, StringComparison.OrdinalIgnoreCase);
        }

        private string BuildQueryString(SortedDictionary<string, string> parameters, bool urlEncode)
        {
            var pairs = new List<string>();
            foreach (var param in parameters)
            {
                var value = urlEncode ? HttpUtility.UrlEncode(param.Value) : param.Value;
                pairs.Add($"{param.Key}={value}");
            }
            return string.Join("&", pairs);
        }

        private string ComputeHmacSha512(string message, string secretKey)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    // Helper class để parse callback từ VNPAY
    public class VnpayCallbackData
    {
        public string? vnp_TmnCode { get; set; }
        public string? vnp_Amount { get; set; }
        public string? vnp_BankCode { get; set; }
        public string? vnp_BankTranNo { get; set; }
        public string? vnp_CardType { get; set; }
        public string? vnp_PayDate { get; set; }
        public string? vnp_OrderInfo { get; set; }
        public string? vnp_TransactionNo { get; set; }
        public string? vnp_ResponseCode { get; set; }
        public string? vnp_TransactionStatus { get; set; }
        public string? vnp_TxnRef { get; set; }
        public string? vnp_SecureHash { get; set; }
        public string? vnp_SecureHashType { get; set; }

        // Parse TxnRef để lấy mã đơn hàng gốc
        public int GetOrderId()
        {
            if (string.IsNullOrEmpty(vnp_TxnRef))
                return 0;

            var parts = vnp_TxnRef.Split('_');
            if (parts.Length > 0 && int.TryParse(parts[0], out var orderId))
                return orderId;
            
            return 0;
        }

        // Kiểm tra thanh toán thành công
        public bool IsSuccess => vnp_ResponseCode == "00" && vnp_TransactionStatus == "00";
    }
}
