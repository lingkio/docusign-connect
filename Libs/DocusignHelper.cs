
using System;
using Lingk_SAML_Example.Common;
using Lingk_SAML_Example.DTO;
using Newtonsoft.Json;
using RestSharp;

namespace Lingk_SAML_Example.Utils
{
    public class DocusignHelper
    {
        public static DateTime? ExpireIn { get; set; }
        private static string AccessToken;
        public static string GetAccessToken(LingkProject lingkProject)
        {
            if (ExpireIn == null || DateTime.Now.Subtract(TimeSpan.FromMinutes(3)) > ExpireIn.Value)
            {
                var resp = LingkHelper.LingkServicecall(lingkProject);

                var lingkDocusign = JsonConvert.DeserializeObject<LingkDocusign>(resp.Result);
                //TODO: jwt will get from above api call
                var jwtGrant = new JwtGrant
                {
                    grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    assertion = lingkDocusign.credentialsJson.JWT
                };

                var client = new RestClient(LingkConst.DocusignAuthUrl);
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