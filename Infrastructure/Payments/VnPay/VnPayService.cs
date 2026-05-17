using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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

    public string CreatePaymentUrl(HttpContext context, int orderId, long amount)
    {
        var data = new SortedList<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _options.TmnCode },
            { "vnp_Amount", (amount * 100).ToString() },
            { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1" },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", $"Thanhtoanhoadon{orderId}" },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", _options.ReturnUrl },
            { "vnp_TxnRef", orderId.ToString() },
            { "vnp_BankCode", "VNPAYQR" }
        };

        // 🔥 1. RAW DATA (KHÔNG encode)
        var rawData = string.Join("&", data.Select(x =>
            $"{x.Key}={x.Value}"));

        // 🔥 2. HASH từ rawData
        var hash = HmacSHA512(_options.HashSecret, rawData);

        // 🔥 3. ENCODE để build URL (chỉ encode ở đây)
        var query = string.Join("&", data.Select(x =>
            $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

        return $"{_options.BaseUrl}?{query}&vnp_SecureHash={hash}";
    }

    public bool ValidateSignature(IQueryCollection query)
    {
        var data = new SortedList<string, string>();

        foreach (var (key, value) in query)
        {
            if (!string.IsNullOrEmpty(key)
                && key.StartsWith("vnp_")
                && key != "vnp_SecureHash"
                && key != "vnp_SecureHashType")
            {
                data.Add(key, value.ToString() ?? "");
            }
        }

        // 🔥 KHÔNG encode lại
        var rawData = string.Join("&", data.Select(x =>
            $"{x.Key}={x.Value}"));

        var hash = HmacSHA512(_options.HashSecret, rawData);

        var vnpSecureHash = query["vnp_SecureHash"].ToString();

        return hash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase);
    }

    public string GetResponseData(IQueryCollection query, string key)
    {
        return query[key].ToString() ?? "";
    }

    private string HmacSHA512(string key, string input)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}