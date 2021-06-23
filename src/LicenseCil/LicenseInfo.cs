using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LicenseCil
{
    // LicenseInfo myDeserializedClass = JsonSerializer.Deserialize<LicenseInfo>(myJsonResponse);
    public class LicenseInfo
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("spdx_id")]
        public string? SpdxId { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("node_id")]
        public string? NodeId { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("implementation")]
        public string? Implementation { get; set; }

        [JsonPropertyName("permissions")]
        public List<string?>? Permissions { get; set; }

        [JsonPropertyName("conditions")]
        public List<string?>? Conditions { get; set; }

        [JsonPropertyName("limitations")]
        public List<string?>? Limitations { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("featured")]
        public bool Featured { get; set; }
    }



}
