using System.Collections;
using System.Text.Json.Serialization;

namespace LicenseCli.Models;
public sealed record LicenseDeclare
{
    [JsonConstructor]
    public LicenseDeclare(
        string reference,
        bool isDeprecatedLicenseId,
        string detailsUrl,
        int referenceNumber,
        string name,
        string licenseId,
        List<string> seeAlso,
        bool isOsiApproved,
        bool? isFsfLibre
    )
    {
        Reference = reference;
        IsDeprecatedLicenseId = isDeprecatedLicenseId;
        DetailsUrl = detailsUrl;
        ReferenceNumber = referenceNumber;
        Name = name;
        LicenseId = licenseId;
        SeeAlso = seeAlso;
        IsOsiApproved = isOsiApproved;
        IsFsfLibre = isFsfLibre;
    }

    [JsonPropertyName("reference")]
    public string Reference { get; }

    [JsonPropertyName("isDeprecatedLicenseId")]
    public bool IsDeprecatedLicenseId { get; }

    [JsonPropertyName("detailsUrl")]
    public string DetailsUrl { get; }

    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("licenseId")]
    public string LicenseId { get; }

    [JsonPropertyName("seeAlso")]
    public IReadOnlyList<string> SeeAlso { get; }

    [JsonPropertyName("isOsiApproved")]
    public bool IsOsiApproved { get; }

    [JsonPropertyName("isFsfLibre")]
    public bool? IsFsfLibre { get; }
}

public sealed record LicensesIndex: IEnumerable<LicenseDeclare>
{
    [JsonConstructor]
    public LicensesIndex(
        string licenseListVersion,
        List<LicenseDeclare> licenses,
        string releaseDate
    )
    {
        LicenseListVersion = licenseListVersion;
        Licenses = licenses;
        ReleaseDate = releaseDate;
    }

    [JsonPropertyName("licenseListVersion")]
    public string LicenseListVersion { get; }

    [JsonPropertyName("licenses")]
    public IReadOnlyList<LicenseDeclare> Licenses { get; }

    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; }

    public IEnumerator<LicenseDeclare> GetEnumerator() => Licenses.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Licenses).GetEnumerator();
}

