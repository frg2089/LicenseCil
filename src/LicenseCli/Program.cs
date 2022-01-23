using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
        foreach (var license in (await GetLicensesIndex(client))?.Licenses!)
        {
            if (Regex.IsMatch(license.LicenseId!, match))
                Console.WriteLine($" {license.LicenseId,-20} | {license.Name} ");
        }
    }

    public static async Task UseLicense(string licenseId, string? authors = default, bool noOutput = false)
    {
        string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".cache");
        string localLicensePath = Path.Combine(cacheDir, $"{licenseId.ToUpper()}.TEMPLATE");
        if (!File.Exists(localLicensePath))
        {
            using HttpClient client = GetHttpClient();

            LicenseDeclare? licenseDeclare = (await GetLicensesIndex(client)).Licenses?.FirstOrDefault(x => string.Equals(x.LicenseId, licenseId, StringComparison.OrdinalIgnoreCase));

            if (licenseDeclare is null)
            {
                throw new InvalidOperationException($"License \"{licenseId}\" are not found. ");
            }

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
                    .Where(x => !x.Equals("var", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Split('='))
                    .ToDictionary(i => i[0], i => i[1].Trim('"'));

                if (varMap["name"].Equals("copyright", StringComparison.OrdinalIgnoreCase))
                {
                    line = varRegex.Replace(line, varMap["original"].Replace("<year>", year));
                    if (!string.IsNullOrWhiteSpace(authors))
                        line = line.Replace("<copyright holders>", authors);
                }
                else
                    line = varRegex.Replace(line, varMap["original"]);
            }
            if (!noOutput)
                Console.WriteLine(line);
            sw.WriteLine(line);
        }

    }

    public static async Task ListLicenses()
    {
        using HttpClient client = GetHttpClient();
        var licenses = (await GetLicensesIndex(client))?.Licenses!;
        int lenLicenseId = licenses.Max(i => i.LicenseId!.Length);

        int i = lenLicenseId - 4;
        int j = i / 2;
        i -= j;

        Console.Write("\x1b[38;2;0;255;255m");
        Console.Write(new string(' ', i));
        Console.Write("SPDX");
        Console.Write(new string(' ', j));
        Console.Write(" |Attr| License Name");
        Console.WriteLine();
        Console.Write(new string('-', i + j + 5));
        Console.Write("+----+");
        Console.Write("--------------------");
        Console.WriteLine();

        foreach (var license in licenses.OrderBy(i => i.LicenseId, StringComparer.OrdinalIgnoreCase)!)
        {
            i = lenLicenseId - license.LicenseId!.Length;
            j = i / 2;
            i -= j;

            string color = license.IsDeprecatedLicenseId ? "\x1b[38;2;255;0;128m" : "\x1b[38;2;0;255;0m";
            Console.Write(color);
            Console.Write(new string(' ', i));
            Console.Write(license.LicenseId);
            Console.Write(new string(' ', j));
            Console.Write(" | ");
            Console.Write(license.IsFsfLibre == true ? $"\x1b[38;2;255;0;0mF{color}" : " ");
            Console.Write(license.IsOsiApproved == true ? $"\x1b[38;2;0;233;255mO{color}" : " ");
            Console.Write(" | ");
            Console.Write(license.Name);


            Console.WriteLine();
        }
        Console.Write("\x1b[0m");
    }

    private static HttpClient GetHttpClient()
    {
        HttpClient client = new();
        string userAgent = $"LicenseCli/{typeof(Program).Assembly.GetName().Version} ({Environment.OSVersion}) dotnet {Environment.Version}";
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        return client;
    }

    private static async Task<LicensesIndex> GetLicensesIndex(HttpClient client)
    {
        await using Stream stream = await TryGetLicensesIndexCache(client);

        LicensesIndex? licenses = await JsonSerializer.DeserializeAsync<LicensesIndex>(stream);
        return licenses switch
        {
            null => throw new InvalidDataException("Faild."),
            _ => licenses
        };
    }

    private static async Task<Stream> TryGetLicensesIndexCache(HttpClient client)
    {
        string cache = Path.Join(AppDomain.CurrentDomain.BaseDirectory, ".cache", "index");
        if (File.Exists(cache))
        {
            var time = File.GetLastWriteTimeUtc(cache);
            if ((DateTime.UtcNow - time) < TimeSpan.FromTicks(TimeSpan.TicksPerDay))
            {
                Console.Error.WriteLine("Get From Cache.");
                return File.OpenRead(cache);
            }
        }
        Console.Error.WriteLine("Get From spdx.org.");
        return await CreateCache(client, cache);

    }

    private static async Task<Stream> CreateCache(HttpClient client, string cache)
    {
        FileStream fs = File.Create(cache);
        await using Stream response = await GetLicensesIndexOnline(client);
        await response.CopyToAsync(fs);
        fs.Seek(0, SeekOrigin.Begin);
        return fs;
    }

    private static async Task<Stream> GetLicensesIndexOnline(HttpClient client)
    {
        const string URL = "https://spdx.org/licenses/licenses.json";

        return await client.GetStreamAsync(URL);
    }
}
