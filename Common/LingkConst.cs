using System;

namespace Lingk_SAML_Example.Common
{
    public class LingkConst
    {
        public static string DocusignDemoUrl = "https://demo.docusign.net/restapi/";
        public static string SessionKey = "Templatepath";

        public static string DocusignReturnUrl = "https://localhost:3002?state=123";
        public static string TempSettingsPath = AppDomain.CurrentDomain.BaseDirectory + "./setting.json";
        public static string LingkFileSystemPath = AppDomain.CurrentDomain.BaseDirectory + "./lingkEnvelop.json";
        public static string YamlConfigPath = "./example-docusign.yaml";
        public static string ClaimsUrl = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";

        public static string DocusignAuthUrl = "https://account-d.docusign.com/oauth/token";
    }
}