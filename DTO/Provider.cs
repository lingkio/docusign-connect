using Newtonsoft.Json;

namespace Docusign_Connect.DTO
{
    public partial class Provider
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("database")]
        public string Database { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("port")]
        public long Port { get; set; }
    }
}