
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DocuSign.Click.Client;

public class DocusignHelper
{

    public static DateTime? ExpireIn { get; set; }
    private static string AccessToken;
    public static string PrepareFullPrivateKeyFilePath(string fileName)
    {
        const string DefaultRSAPrivateKeyFileName = "./../docusignPrivatKey.txt";

        var fileNameOnly = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(fileNameOnly))
        {
            fileNameOnly = DefaultRSAPrivateKeyFileName;
        }

        var filePath = Path.GetDirectoryName(fileName);
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = Directory.GetCurrentDirectory();
        }

        return Path.Combine(filePath, fileNameOnly);
    }

    public static byte[] ReadFileContent(string path)
    {
        return File.ReadAllBytes(path);
    }

    public static string GetAccessToken(string privateKey)
    {
        if (ExpireIn == null || DateTime.Now.Subtract(TimeSpan.FromMinutes(3)) > ExpireIn.Value)
        {
            //TODO: need to make the call this will remove the blow hardcoded value
            // var result = LingkHelper.LingkServicecall("/v1/@self/ps/credentials", "GET",
            // _lingkConfig.LingkProject.ClientId, _lingkConfig.LingkProject.ClientSecret);
            var apiClient = new ApiClient();
            var authToken = apiClient.RequestJWTUserToken(
                "f249774c-4731-48f7-9fbd-0d79f241fcb2",
               "94f03974-2e9f-4b44-9652-d3681a9ea150",
               "account-d.docusign.com",
               ReadFileContent(PrepareFullPrivateKeyFilePath(privateKey)),
               1,
               new List<string> { "signature", "impersonation" }
            );

            AccessToken = authToken.access_token;
            ExpireIn = DateTime.Now.AddSeconds(authToken.expires_in.Value);
            //TODO: need to check the expiration of token and regenerate
            return AccessToken;
        }
        return AccessToken;

    }

}