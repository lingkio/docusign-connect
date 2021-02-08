using Newtonsoft.Json;
namespace Docusign_Connect.DTO
{
    public partial class LingkConfig
    {
        [JsonProperty("authn")]
        public Authn Authn { get; set; }

        [JsonProperty("lingkProject")]
        public LingkProject LingkProject { get; set; }

        [JsonProperty("providers")]
        public Provider[] Providers { get; set; }

        [JsonProperty("envelopes")]
        public Envelope[] Envelopes { get; set; }
    }

    public partial class Authn
    {
        [JsonProperty("saml")]
        public Saml Saml { get; set; }
    }

    public partial class Saml
    {
        [JsonProperty("metadataLocal")]
        public string MetadataLocal { get; set; }

        [JsonProperty("issuerId")]
        public string IssuerId { get; set; }

        [JsonProperty("signatureDigest")]
        public string SignatureDigest { get; set; }

    }
    public partial class Envelope
    {
        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("linkFromSamlToProvider")]
        public string LinkFromSamlToProvider { get; set; }

        [JsonProperty("tabs")]
        public Tab[] Tabs { get; set; }

        [JsonProperty("docusignReturnUrl")]
        public string DocusignReturnUrl { get; set; }

    }

    public partial class Tab
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sourceDataField")]
        public string SourceDataField { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }


        [JsonProperty("table")]
        public string Table { get; set; }

    }

    public partial class LingkProject
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("environmentKey")]
        public string EnvironmentKey { get; set; }

        [JsonProperty("entrypoint")]
        public string Entrypoint { get; set; }
    }

    public partial class LingkEnvelope
    {
        public string templateId { get; set; }
        public string accountId { get; set; }
        public string envelopeId { get; set; }
        public string recipientUrl { get; set; }
        public string envelopePath { get; set; }
    }
}