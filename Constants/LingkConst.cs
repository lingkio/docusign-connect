using System;

namespace Lingk_SAML_Example.Constants
{
    public class LingkConst
    {
        public static string DocusignDemoUrl = "https://demo.docusign.net/restapi/";
        public static string DocusignProdUrl = "https://www.docusign.net/restapi/";
        public static string SessionKey = "Templatepath";
        public static string DocuSignGrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        public static string TempSettingsPath = AppDomain.CurrentDomain.BaseDirectory + "./setting.json";
        public static string LingkFileSystemPath = AppDomain.CurrentDomain.BaseDirectory + "./lingkEnvelope.json";
        public static string ClaimsUrl = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";
        public static string DocusignDemoAuthUrl = "https://account-d.docusign.com/oauth/token";
        public static string DocusignProdAuthUrl = "https://account.docusign.com/oauth/token";
    }
}