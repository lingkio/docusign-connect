using System;
using System.Security.Cryptography;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Threading.Tasks;

public class LingkHelper
{
    static String entrypoint = "http://www.lingkapis.com";
    private static readonly Encoding encoding = Encoding.UTF8;
    private static string CreateSignature(string message, string secret)
    {
        secret = secret ?? "";
        var encoding = new System.Text.ASCIIEncoding();
        byte[] keyByte = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);
        using (var hmacsha1 = new HMACSHA1(keyByte))
        {
            byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
    }

    public static async Task<string> LingkServicecall(String service, String method,
     string apikey, string secret)
    {
        String requestDate;
        UriBuilder uri = new UriBuilder(entrypoint + service);
        Console.WriteLine(uri);
        requestDate = DateTime.UtcNow.ToString("R");

        HttpClientHandler handler = new HttpClientHandler() { };
        string fullMessage = "date: " + requestDate + "\n(request-target): " + method.ToLower() + " " + service;
        using (var client = new HttpClient(handler))
        {
            string hashString = CreateSignature(fullMessage, secret);
            String authHeader = "Signature keyId=\"" + apikey + "\",headers=\"date (request-target)\",algorithm=\"hmac-sha1\",signature=\"" + WebUtility.UrlEncode(hashString) + "\"";

            client.BaseAddress = new System.Uri(entrypoint);
            client.DefaultRequestHeaders.Add("Date", requestDate);
            client.DefaultRequestHeaders.Add("Authorization", authHeader);
            var resp2 = await client.GetAsync(service);
            var aaa = resp2.Content;
            string result = await aaa.ReadAsStringAsync();
            return result;
        }
    }
}