using System.Text.Json.Serialization;

namespace LicenseCli.Models;
public sealed record CrossRef
{
    [JsonPropertyName("match")]
    public string? Match { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("isLive")]
    public bool IsLive { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("isWayBackLink")]
    public bool IsWayBackLink { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public sealed record LicenseDetails
{
    [JsonPropertyName("isDeprecatedLicenseId")]
    public bool IsDeprecatedLicenseId { get; set; }

    [JsonPropertyName("isFsfLibre")]
    public bool IsFsfLibre { get; set; }

    [JsonPropertyName("licenseText")]
    public string? LicenseText { get; set; }

    [JsonPropertyName("standardLicenseTemplate")]
    public string? StandardLicenseTemplate { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("licenseId")]
    public string? LicenseId { get; set; }

    [JsonPropertyName("crossRef")]
    public List<CrossRef>? CrossRef { get; set; }

    [JsonPropertyName("seeAlso")]
    public List<string>? SeeAlso { get; set; }

    [JsonPropertyName("isOsiApproved")]
    public bool IsOsiApproved { get; set; }

    [JsonPropertyName("licenseTextHtml")]
    public string? LicenseTextHtml { get; set; }
}

