namespace Lingk_SAML_Example.DTO
{
    public class CredentialsJson
    {
        public string JWT { get; set; }
        public string consumerKey { get; set; }
        public bool isSandbox { get; set; }
        public string name { get; set; }
        public string userName { get; set; }
    }

    public class LingkDocusign
    {
        public string credentialType { get; set; }
        public string credentials { get; set; }
        public CredentialsJson credentialsJson { get; set; }
        public string providerLabel { get; set; }
        public string key { get; set; }
        public bool api_accessible { get; set; }
    }
}