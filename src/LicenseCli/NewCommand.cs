using System.Text;
using System.Text.RegularExpressions;

using DotMake.CommandLine;

using LicenseCli.Models;
using LicenseCli.Resources;

namespace LicenseCli;

[CliCommand(Aliases = ["create", "use"], Description = nameof(Resource.New_Description), Parent = typeof(RootCommand))]
internal sealed partial class NewCommand
{
    [CliArgument(Description = nameof(Resource.SPDXExpression_Description), Name = "SPDX Expression", Required = true)]
    public required string SPDXExpression { get; set; }

    [CliArgument(Description = nameof(Resource.Author_Description), Required = false)]
    public string? Author { get; set; }

    [CliOption(Description = nameof(Resource.Output_Description), Aliases = ["-o", "--out"], Required = false)]
    public string Output { get; set; } = Path.Combine(Environment.CurrentDirectory, "LICENSE");

    [CliOption(Description = nameof(Resource.Quiet_Description), Aliases = ["-q"], Required = false)]
    public bool Quiet { get; set; }

    [CliOption(Description = nameof(Resource.PrintWidth_Description), Required = false)]
    public int PrintWidth { get; set; } = 80;

    [CliOption(Description = nameof(Resource.NoOptional_Description), Required = false)]
    public bool NoOptional { get; set; }

    //[CliOption(Description = nameof(Resource.Variables_Description), Required = false)]
    public Dictionary<string, string> Variables { get; set; } = [];

#if NET8_0_OR_GREATER
    [GeneratedRegex("<<.+?>>")]
    private static partial Regex SlotRegex();
#else
    private static Regex SlotRegex() => new("<<.+?>>");
#endif

    public async Task RunAsync()
    {
        using SPDXClient client = new();
        var license = await client.GetLicenseAsync(SPDXExpression).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(license?.StandardLicenseTemplate))
            throw new InvalidDataException("Cannot found StandardLicenseTemplate");

        var slotRegex = SlotRegex();
        var templates = slotRegex.Split(license.StandardLicenseTemplate);
        var slots = slotRegex.Matches(license.StandardLicenseTemplate);

        StringBuilder result = new(templates[0]);
        bool inOptional = false;
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var value = slot.Value.TrimStart('<').TrimEnd('>');
            if (value is "beginOptional")
                inOptional = true;
            else if (value is "endOptional")
                inOptional = false;
            else if (SPDXVar.TryParse(value, out var var))
            {
                if (!Variables.TryGetValue(var.Name, out var varValue))
                {
                    varValue = var.Original;
                    if (var.Name is "copyright")
                    {
                        varValue = varValue.Replace("<year>", DateTimeOffset.Now.Year.ToString());
                        if (!string.IsNullOrWhiteSpace(Author))
                            varValue = varValue.Replace("<copyright holders>", Author);
                    }
                }

                if (!var.Match.IsMatch(varValue))
                {
                    Console.Error.WriteLine($"Cannot set \"{var.Name}\". Because the value \"{varValue}\" are not matched \"{var.Match}\"");
                    varValue = var.Original;
                }

                result.Append(varValue);
            }
            else
            {
                Console.WriteLine(value);
            }


            // 不在 <<beginOptional>> 和 <<endOptional>> 之间
            // 或允许输出 <<beginOptional>> 和 <<endOptional>> 之间的内容
            if (!inOptional || !NoOptional)
            {
                var template = templates[i + 1];
                result.Append(template);
            }
        }

        if (PrintWidth is not 0)
        {
            StringReader sr = new(result.ToString());
            result.Clear();
            while (sr.Peek() is not -1)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.Length < PrintWidth)
                {
                    result.AppendLine(line ?? string.Empty);
                    continue;
                }

                int indent = 0;
                for (; indent < line.Length; indent++)
                {
                    if (!char.IsWhiteSpace(line[indent]))
                        break;
                }
                line = line[indent..];
                while (line.Length > 0)
                {
                    var p = PrintWidth;
                    p -= indent;
                    if (p >= line.Length)
                        break;

                    if (!char.IsWhiteSpace(line[p]))
                    {
                        for (p -= 1; p >= 0; p--)
                        {
                            if (char.IsWhiteSpace(line[p]))
                                break;
                        }
                    }

                    if (indent is not 0)
                        result.Append(new string(' ', indent));

                    result.AppendLine(line[..p]);
                    line = line[(p + 1)..];
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (indent is not 0)
                        result.Append(new string(' ', indent));

                    result.AppendLine(line);
                }
            }
        }

        if (!Quiet)
            Console.WriteLine(result);
        await File.WriteAllTextAsync(Output, result.ToString()).ConfigureAwait(false);
    }
}
