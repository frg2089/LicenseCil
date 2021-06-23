using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LicenseCil
{
    static class Program
    {
        const string URL = "https://api.github.com/licenses";
        static readonly HttpClient httpClient = new();
        static readonly DirectoryInfo cacheDir = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"));

        static async Task Main(string[] args)
        {
            var userAgent = $"LicenseCli/{typeof(Program).Assembly.GetName().Version} ({Environment.OSVersion}) dotnet {Environment.Version}";
            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "update":
                        await Update();
                        break;
                    case "new":
                    case "use":
                        if (args.Length > 1)
                            if (args.Length > 2)
                                await UseLicense(args[1].ToLower(), args[2]);
                            else
                                await UseLicense(args[1].ToLower());
                        else
                            goto default;
                        break;
                    default:
                        Help();
                        break;
                }
            }
        }
        static void Help()
        {
            Console.WriteLine("Create License Tool");
            Console.WriteLine("Usage:");
            Console.WriteLine("    update                  : Update cache from Github");
            Console.WriteLine("    new <license> [author]  : Create License");
        }
        static async Task Update()
        {
            var response = await httpClient.GetAsync(URL);
            var json = await response.Content.ReadAsStringAsync();
            var licenses = JsonSerializer.Deserialize<License[]>(json);
            if (licenses is null)
                return;

            Console.WriteLine($"|{"-".PadCenter(20, '-')}+{"-".PadRight(50, '-')}|");
            Console.WriteLine($"|{"ID".PadCenter(20)}|{" Name",-50}|");
            Console.WriteLine($"|{"-".PadCenter(20, '-')}+{"-".PadRight(50, '-')}|");
            foreach (var license in licenses)
                Console.WriteLine($"|{license.Key.PadCenter(20)}| {license.Name,-49}|");

            Console.WriteLine($"|{"-".PadCenter(20, '-')}+{"-".PadRight(50, '-')}|");

            if (!cacheDir.Exists)
                cacheDir.Create();

            await Task.WhenAll(licenses.Select(async license =>
            {
                var licenseInfoJson = await httpClient.GetAsync(license.Url);
                FileInfo file = new(Path.Combine(cacheDir.FullName, license.Key!));
                using var fs = file.Create();
                await licenseInfoJson.Content.CopyToAsync(fs);
            }));
        }

        static async Task UseLicense(string key, string fullname = "")
        {
            if (!cacheDir.Exists)
            {
                Console.Error.WriteLine("Please update at first!");
                return;
            }

            FileInfo file = new(Path.Combine(cacheDir.FullName, key));
            if (!file.Exists)
            {
                Console.Error.WriteLine($"Cannot found License {key}");
                var licenseCaches = cacheDir.GetFiles();
                Console.WriteLine($"|{"-".PadCenter(20, '-')}+{"-".PadRight(50, '-')}|");
                Console.WriteLine($"|{"ID".PadCenter(20)}|{" Name",-50}|");
                Console.WriteLine($"|{"-".PadCenter(20, '-')}+{"-".PadRight(50, '-')}|");
                foreach (var licenseCache in licenseCaches)
                {
                    using var sr = licenseCache.OpenText();
                    var license = JsonSerializer.Deserialize<LicenseInfo>(await sr.ReadToEndAsync());
                    if (license is not null)
                        Console.WriteLine($"|{license.Key.PadCenter(20)}| {license.Name,-49}|");
                }

                Console.WriteLine($"|{"-".PadCenter(20, '-')}+{"-".PadRight(50, '-')}|");
                return;
            }
            else
            {
                using var sr = file.OpenText();
                var licenseInfo = JsonSerializer.Deserialize<LicenseInfo>(await sr.ReadToEndAsync())!;

                Console.WriteLine(licenseInfo.Name);
                Console.WriteLine(licenseInfo.Description);

                FileInfo target = new(Path.Combine(Environment.CurrentDirectory, "LICENSE"));
                using StreamWriter sw = new(target.OpenWrite());
                var license = licenseInfo.Body!.Replace("[year]", DateTime.Now.Year.ToString());
                if (!string.IsNullOrWhiteSpace(fullname))
                    license = license.Replace("[fullname]", fullname);

                await sw.WriteAsync(license.Replace("\n", Environment.NewLine));
            }
        }

        public static string PadCenter(this string? @this, int totalWidth, char paddingChar = ' ')
        {
            if (string.IsNullOrEmpty(@this))
                return string.Empty;
            var space = totalWidth - @this.Length;
            var left = (space / 2) + @this.Length;
            return @this.PadLeft(left, paddingChar).PadRight(space + @this.Length, paddingChar);
        }
    }



}
