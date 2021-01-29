using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Converters;

namespace Lingk_SAML_Example.Common
{
    public partial class LingkConfig
    {
        [JsonProperty("authn")]
        public Authn Authn { get; set; }

        [JsonProperty("lingkProject")]
        public LingkProject LingkProject { get; set; }

        [JsonProperty("envelopes")]
        public Envelope[] Envelopes { get; set; }

        [JsonProperty("docusignCrd")]
        public DocusignCrd DocusignCrd { get; set; }
    }

    public partial class Authn
    {
        [JsonProperty("saml")]
        public Saml Saml { get; set; }
    }

    public partial class Saml
    {
        [JsonProperty("keyStore")]
        public string KeyStore { get; set; }

        [JsonProperty("keyStoreAliasName")]
        public string KeyStoreAliasName { get; set; }

        [JsonProperty("keyStorePassword")]
        public string KeyStorePassword { get; set; }

        [JsonProperty("metadataLocal")]
        public string MetadataLocal { get; set; }

        [JsonProperty("issuerId")]
        public string IssuerId { get; set; }

        [JsonProperty("serviceAddress")]
        public Uri ServiceAddress { get; set; }
    }

    public partial class DocusignCrd
    {
        [JsonProperty("privateKey")]
        public string PrivateKey { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }
    }
    public partial class Envelope
    {
        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("security")]
        public string Security { get; set; }

        [JsonProperty("tabs")]
        public Tab[] Tabs { get; set; }
    }

    public partial class Tab
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sourceDataField")]
        public string SourceDataField { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class LingkProject
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
    }

    public partial class LingkEnvelop
    {
        public string templateId { get; set; }
        public string accountId { get; set; }
        public string envelopId { get; set; }
        public string recipientUrl { get; set; }
    }
}