
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DocuSign.Click.Client;
using Lingk_SAML_Example.DTO;
using Newtonsoft.Json;
using RestSharp;

namespace Lingk_SAML_Example.Utils
{
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

                //TODO: jwt will get from above api call
                var jwtGrant = new JwtGrant
                {
                    grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    assertion = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJhY2NvdW50LWQuZG9jdXNpZ24uY29tIiwiZXhwIjoxNjEyMjQ5NTYzLCJpYXQiOjE2MTIyNDkyNjMsImlzcyI6ImYyNDk3NzRjLTQ3MzEtNDhmNy05ZmJkLTBkNzlmMjQxZmNiMiIsInNjb3BlIjoic2lnbmF0dXJlIGltcGVyc29uYXRpb24iLCJzdWIiOiI5NGYwMzk3NC0yZTlmLTRiNDQtOTY1Mi1kMzY4MWE5ZWExNTAifQ.dMTpXtprxPOZN2FZUo4saG0PK0BL3nSTQUOhsB3ZrQ1HkL2Lxpzed2yVi3V7ZKnycoRx9mlownI5d-EOHNHXSkRhGnbxr-d6ZSeK4F9ciQUoEORRkUSb-rtsBTET95Cle2MnxZSjuTCmeul-HgGYqaI614ABBWItnflFtmzXEu3Hog5TjNDVnGqODqGNgAcIl-vJR8FeF9dT0sJharjRDOg7xGk5ohXxF4jsg1VkkR3wBNU4lBhXFhYBImYq58or38kQUDL_Kqi5gSs4wKK1bbbqgWDbiFd5dOR_9zcg6eWqHi8VXE7xCTI2slo9hJioxO45gFYNXljDtRzVvYquyA"
                };

                var client = new RestClient("https://account-d.docusign.com/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("application/json", JsonConvert.SerializeObject(jwtGrant), ParameterType.RequestBody);
                IRestResponse responseData = client.Execute(request);
                var authToken = JsonConvert.DeserializeObject<Token>(responseData.Content);
                if (authToken.error != null)
                {
                    throw new Exception(authToken.error_description + " not able to generate access_token from jwt ");
                }
                AccessToken = authToken.access_token;
                ExpireIn = DateTime.Now.AddSeconds(authToken.expires_in);
                return AccessToken;
            }
            return AccessToken;

        }

    }
}