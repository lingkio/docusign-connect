using System;

namespace Docusign_Connect.Constants
{
    public class LingkConst
    {
        public static string DocusignDemoUrl = "https://demo.docusign.net/restapi/";
        public static string DocusignProdUrl = "https://www.docusign.net/restapi/";
        public static string SessionKey = "Templatepath";
        public static string DocuSignGrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        public static string LingkFileSystemPath = AppDomain.CurrentDomain.BaseDirectory + "./lingkEnvelope.json";
        public static string ClaimsUrl = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";
        public static string ClaimsUrlWithYear = "http://schemas.microsoft.com/ws/2008/06/identity/claims/";
        public static string SamlClaimBaseUrl = "http://schemas.auth0.com/";
        public static string IdentitiesDefault = "http://schemas.auth0.com/identities/default/";
        public static string DocusignDemoAuthUrl = "https://account-d.docusign.com/oauth/token";
        public static string DocusignProdAuthUrl = "https://account.docusign.com/oauth/token";
    }
}