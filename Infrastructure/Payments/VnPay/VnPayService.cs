using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RoomManagement.WebAPI.Infrastructure.Payments.VnPay;

public class VnPayService : IVnPayService
{
    private readonly VnPayOptions _options;

    public VnPayService(IOptions<VnPayOptions> options)
    {
        _options = options.Value;
    }

    public string CreatePaymentUrl(HttpContext context, string txnRef, long amount)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1")
        {
            ipAddress = "127.0.0.1";
        }

        var data = new SortedDictionary<string, string>(new VnPayCompare())
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _options.TmnCode },
            { "vnp_Amount", checked(amount * 100).ToString(CultureInfo.InvariantCulture) },
            { "vnp_CreateDate", DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
                .ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", ipAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", $"Thanhtoanhoadon{txnRef}" },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", _options.ReturnUrl },
            { "vnp_TxnRef", txnRef }
            // Không gán cứng vnp_BankCode để VNPay mở trang chọn phương thức thanh toán
        };

        // Chuẩn VNPay 2.1.0: Dùng WebUtility.UrlEncode cho cả key và value trước khi băm
        var query = string.Join("&", data.Select(x =>
            $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

        var hash = HmacSHA512(_options.HashSecret, query);

        return $"{_options.BaseUrl}?{query}&vnp_SecureHash={hash}";
    }

    public bool ValidateSignature(IQueryCollection query)
    {
        var data = new SortedDictionary<string, string>(new VnPayCompare());

        foreach (var (key, value) in query)
        {
            if (!string.IsNullOrEmpty(key)
                && key.StartsWith("vnp_")
                && key != "vnp_SecureHash"
                && key != "vnp_SecureHashType")
            {
                var val = value.ToString();
                if (!string.IsNullOrEmpty(val))
                {
                    data.TryAdd(key, val);
                }
            }
        }

        var receivedHash = query["vnp_SecureHash"].ToString();
        if (string.IsNullOrWhiteSpace(receivedHash))
            return false;

        var rawData = string.Join("&", data.Select(x =>
            $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

        var expectedHash = HmacSHA512(_options.HashSecret, rawData);
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(expectedHash),
                Convert.FromHexString(receivedHash));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public string GetResponseData(IQueryCollection query, string key)
    {
        return query[key].ToString() ?? "";
    }

    private string HmacSHA512(string key, string input)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
