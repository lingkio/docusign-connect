using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Converters;

namespace Lingk_SAML_Example.DTO
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

        [JsonProperty("serviceAddress")]
        public Uri ServiceAddress { get; set; }

        [JsonProperty("signatureDigest")]
        public string SignatureDigest { get; set; }

    }
    public partial class Envelope
    {
        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("linkFromProviderToSaml")]
        public string LinkFromProviderToSaml { get; set; }

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

    public partial class LingkEnvelop
    {
        public string templateId { get; set; }
        public string accountId { get; set; }
        public string envelopId { get; set; }
        public string recipientUrl { get; set; }
    }
}