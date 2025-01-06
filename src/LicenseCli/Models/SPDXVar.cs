using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace LicenseCli.Models;
internal sealed record class SPDXVar(string Name, string Original, Regex Match)
{
    public static bool TryParse(string input, [NotNullWhen(true)] out SPDXVar? var)
    {
        var = default;
        input = input.TrimStart('<').TrimEnd('>');
        if (!input.StartsWith("var"))
            return false;

        var fields = input[4..]
            .Split(';')
            .Select(i => i.Split('='))
            .ToDictionary(
                i => i[0],
                i => i[1]
                    .Trim('"')
                    .Replace("\\\"", "\""));

        var = new(
            fields["name"],
            fields["original"],
            new(fields["match"])
        );

        return true;
    }
}
