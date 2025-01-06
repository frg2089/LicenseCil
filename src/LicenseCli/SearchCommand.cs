using DotMake.CommandLine;

using LicenseCli.Resources;
using ConsoleTables;
using LikeComparison.PostgreSql;

namespace LicenseCli;

[CliCommand(Aliases = ["find"], Description = nameof(Resource.Search_Description), Parent = typeof(RootCommand))]
internal sealed class SearchCommand
{
    [CliArgument(Description = nameof(Resource.SPDXLike_Description), Name = "Like Name", Required = true)]
    public required string Like { get; set; }

    public async Task RunAsync()
    {
        using SPDXClient client = new();
        var index = await client.GetLicenseIndexAsync().ConfigureAwait(false);

        ConsoleTable table = new(Resource.LicenseId, Resource.LicenseName);
        foreach (var license in index.Licenses ?? [])
        {
            if (string.IsNullOrWhiteSpace(license.LicenseId)
                || !Like.ILike(license.LicenseId))
                continue;
            table.AddRow(license.LicenseId, license.Name);
        }

        Console.WriteLine(table.ToMinimalString().Replace('|', '\0'));
    }
}