using System.Text.Json.Serialization;

namespace LicenseCli.Models;
public sealed record CrossRef
{
    [JsonConstructor]
    public CrossRef(
        string match,
        string url,
        bool isValid,
        bool isLive,
        DateTime timestamp,
        bool isWayBackLink,
        int order
    )
    {
        Match = match;
        Url = url;
        IsValid = isValid;
        IsLive = isLive;
        Timestamp = timestamp;
        IsWayBackLink = isWayBackLink;
        Order = order;
    }

    [JsonPropertyName("match")]
    public string Match { get; }

    [JsonPropertyName("url")]
    public string Url { get; }

    [JsonPropertyName("isValid")]
    public bool IsValid { get; }

    [JsonPropertyName("isLive")]
    public bool IsLive { get; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; }

    [JsonPropertyName("isWayBackLink")]
    public bool IsWayBackLink { get; }

    [JsonPropertyName("order")]
    public int Order { get; }
}

public sealed record LicenseDetails
{
    [JsonConstructor]
    public LicenseDetails(
        bool isDeprecatedLicenseId,
        bool isFsfLibre,
        string licenseText,
        string standardLicenseTemplate,
        string name,
        string licenseId,
        List<CrossRef> crossRef,
        List<string> seeAlso,
        bool isOsiApproved,
        string licenseTextHtml
    )
    {
        IsDeprecatedLicenseId = isDeprecatedLicenseId;
        IsFsfLibre = isFsfLibre;
        LicenseText = licenseText;
        StandardLicenseTemplate = standardLicenseTemplate;
        Name = name;
        LicenseId = licenseId;
        CrossRef = crossRef;
        SeeAlso = seeAlso;
        IsOsiApproved = isOsiApproved;
        LicenseTextHtml = licenseTextHtml;
    }

    [JsonPropertyName("isDeprecatedLicenseId")]
    public bool IsDeprecatedLicenseId { get; }

    [JsonPropertyName("isFsfLibre")]
    public bool IsFsfLibre { get; }

    [JsonPropertyName("licenseText")]
    public string LicenseText { get; }

    [JsonPropertyName("standardLicenseTemplate")]
    public string StandardLicenseTemplate { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("licenseId")]
    public string LicenseId { get; }

    [JsonPropertyName("crossRef")]
    public IReadOnlyList<CrossRef> CrossRef { get; }

    [JsonPropertyName("seeAlso")]
    public IReadOnlyList<string> SeeAlso { get; }

    [JsonPropertyName("isOsiApproved")]
    public bool IsOsiApproved { get; }

    [JsonPropertyName("licenseTextHtml")]
    public string LicenseTextHtml { get; }
}

