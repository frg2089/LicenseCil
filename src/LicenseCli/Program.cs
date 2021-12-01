using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using LicenseCli.Models;

namespace LicenseCli;

static class Program
{
    public static void Help()
    {
        Console.WriteLine("Create License Tool");
        Console.WriteLine("Usage:");
        Console.WriteLine("    [create|new|use] <SPDX Expression> [author]  : Create License");
        Console.WriteLine("    [search|find] <match>                        : Find License from spdx.org");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("    -h                                           : Show Help");
        Console.WriteLine("    -q                                           : No Console Output");
    }

    public static async Task SearchLicense(string match)
    {
        using HttpClient client = GetHttpClient();
        foreach (var license in await GetLicensesIndex(client))
        {
            if (Regex.IsMatch(license.LicenseId, match))
                Console.WriteLine($" {license.LicenseId,-20} | {license.Name} ");
        }
    }

    public static async Task UseLicense(string licenseId, string? authors = default, bool noOutput = false)
    {
        string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
        string localLicensePath = Path.Combine(cacheDir, $"{licenseId.ToUpper()}.TEMPLATE");
        if (!File.Exists(localLicensePath))
        {
            using HttpClient client = GetHttpClient();

            LicenseDeclare? licenseDeclare = (await GetLicensesIndex(client)).FirstOrDefault(x => string.Equals(x.LicenseId, licenseId, StringComparison.OrdinalIgnoreCase));

            if (licenseDeclare is null)
                throw new InvalidOperationException($"License \"{licenseId}\" are not found");

            await using Stream licenseStream = await client.GetStreamAsync(licenseDeclare.DetailsUrl);

            LicenseDetails? license = await JsonSerializer.DeserializeAsync<LicenseDetails>(licenseStream);
            if (license is null)
                throw new InvalidDataException("Faild.");

            await File.WriteAllTextAsync(localLicensePath, license.StandardLicenseTemplate, Encoding.UTF8);

        }
        await Task.Yield();

        using StreamReader sr = File.OpenText(localLicensePath);
        using StreamWriter sw = new(File.Create(Path.Combine(Environment.CurrentDirectory, "LICENSE")));

        // <<var;name="title";original="BSD Zero Clause License";match="(BSD Zero[ -]Clause|Zero[ -]Clause BSD)( License)?( \(0BSD\))?">>
        // \<\<var;name=".+?";original=".+?";match=".+?"\>\>
        Regex varRegex = new(@"\<\<var;name="".+?"";original="".+?"";match="".+?""\>\>");
        string year = DateTime.Now.Year.ToString();
        //bool isBeginOptional = false;
        while (!sr.EndOfStream)
        {
            string? line = sr.ReadLine();
            if (line is null)
                throw new("Impossible.");

            if (line.Contains("<<beginOptional>>"))
            {
                line = line.Replace("<<beginOptional>>", string.Empty);
                //isBeginOptional = true;
            }
            if (line.Contains("<<endOptional>>"))
            {
                line = line.Replace("<<endOptional>>", string.Empty);
                //isBeginOptional = false;
            }
            var match = varRegex.Match(line);
            if (match.Success)
            {
                var varMap = match.Value.TrimStart('<').TrimEnd('>').Split(';')
                    .Where(x => x.Equals("var", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Split('='))
                    .ToDictionary(i => i[0], i => i[1].Trim('"'));

                if (varMap["name"].Equals("copyright", StringComparison.OrdinalIgnoreCase))
                    varRegex.Replace(line, $"Copyright © {year} {authors ?? "<copyright holders>"}");
                else
                    varRegex.Replace(line, varMap["original"]);
            }
            if (!noOutput)
                Console.WriteLine(line);
            sw.WriteLine(line);
        }

    }

    private static HttpClient GetHttpClient()
    {
        HttpClient client = new();
        string userAgent = $"LicenseCli/{typeof(Program).Assembly.GetName().Version} ({Environment.OSVersion}) dotnet {Environment.Version}";
        client.DefaultRequestHeaders.UserAgent.Add(new(userAgent));

        return client;
    }

    private static async Task<LicensesIndex> GetLicensesIndex(HttpClient client)
    {
        const string URL = "https://spdx.org/licenses/licenses.json";

        await using Stream response = await client.GetStreamAsync(URL);

        LicensesIndex? licenses = await JsonSerializer.DeserializeAsync<LicensesIndex>(response);
        return licenses switch
        {
            null => throw new InvalidDataException("Faild."),
            _ => licenses
        };
    }
}
