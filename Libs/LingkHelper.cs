using System;
using System.Security.Cryptography;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using Lingk_SAML_Example.DTO;

namespace Lingk_SAML_Example.Utils
{
    public class LingkHelper
    {
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

        public static async Task<string> LingkServicecall(LingkProject lingkProject)
        {
            var service = "/v1/@self/" + lingkProject.EnvironmentKey + "/credentials/DocuSign";
            var method = "GET";
            String requestDate;
            UriBuilder uri = new UriBuilder(lingkProject.Entrypoint + service);
            Console.WriteLine(uri);
            requestDate = DateTime.UtcNow.ToString("R");

            HttpClientHandler handler = new HttpClientHandler() { };
            string fullMessage = "date: " + requestDate + "\n(request-target): " + method.ToLower() + " " + service;
            using (var client = new HttpClient(handler))
            {
                string hashString = CreateSignature(fullMessage, lingkProject.ClientSecret);
                String authHeader = "Signature keyId=\"" + lingkProject.ClientId + "\",headers=\"date (request-target)\",algorithm=\"hmac-sha1\",signature=\"" + WebUtility.UrlEncode(hashString) + "\"";

                client.BaseAddress = new System.Uri(lingkProject.Entrypoint);
                client.DefaultRequestHeaders.Add("Date", requestDate);
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
                var resp2 = await client.GetAsync(service);
                var aaa = resp2.Content;
                string result = await aaa.ReadAsStringAsync();
                return result;
            }
        }
    }
}