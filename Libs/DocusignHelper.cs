
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Docusign_Connect.Constants;
using Docusign_Connect.DTO;
using Newtonsoft.Json;
using System.Linq;
using RestSharp;

namespace Docusign_Connect.Utils
{
    public class DocusignHelper
    {
        public static DateTime? ExpireIn { get; set; }
        private static LingkCredentials lingkCredentials;
        public static LingkCredentials GetAccessToken(LingkProject lingkProject)
        {
            if (lingkCredentials == null || ExpireIn == null || DateTime.Now.Subtract(TimeSpan.FromMinutes(3)) > ExpireIn.Value)
            {
                var resp = LingkHelper.LingkServicecall(lingkProject);

                var lingkDocusign = JsonConvert.DeserializeObject<LingkDocusign>(resp.Result);

                if (lingkDocusign.errorCode != null)
                {
                    throw new Exception(lingkDocusign.errorCode + ":" + lingkDocusign.errorDescription
                    + "," + lingkDocusign.errorMetadata);
                }
                var jwtGrant = new JwtGrant
                {
                    grant_type = LingkConst.DocuSignGrantType,
                    assertion = lingkDocusign.credentialsJson.JWT
                };

                var client = new RestClient(lingkDocusign.credentialsJson.isSandbox ? LingkConst.DocusignDemoAuthUrl : LingkConst.DocusignProdAuthUrl);
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
                ExpireIn = DateTime.Now.AddSeconds(authToken.expires_in);
                lingkCredentials = new LingkCredentials
                {
                    credentialsJson = lingkDocusign.credentialsJson,
                    docuSignToken = authToken
                };
            }
            return lingkCredentials;

        }
    }
}