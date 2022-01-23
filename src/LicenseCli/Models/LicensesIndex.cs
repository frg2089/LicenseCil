using System.Collections;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace LicenseCli.Models;
public sealed record LicenseDeclare
{
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("isDeprecatedLicenseId")]
    public bool IsDeprecatedLicenseId { get; set; }

    [JsonPropertyName("detailsUrl")]
    public string? DetailsUrl { get; set; }

    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("licenseId")]
    public string? LicenseId { get; set; }

    [JsonPropertyName("seeAlso")]
    public List<string>? SeeAlso { get; set; }

    [JsonPropertyName("isOsiApproved")]
    public bool IsOsiApproved { get; set; }

    [JsonPropertyName("isFsfLibre")]
    public bool? IsFsfLibre { get; set; }
}

public sealed record LicensesIndex
{
    [JsonPropertyName("licenseListVersion")]
    public string? LicenseListVersion { get; set; }

    [JsonPropertyName("licenses")]
    public List<LicenseDeclare>? Licenses { get; set; }

    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; set; }
}

