using System.Text.Json;

using LicenseCli.Models;

namespace LicenseCli;
internal sealed class SPDXClient(HttpClient client, bool leaveOpen = false) : IDisposable
{
    private bool _disposedValue;

    private static HttpClient GetHttpClient()
    {
        HttpClient client = new();
        string userAgent = $"LicenseCli/{typeof(Program).Assembly.GetName().Version} ({Environment.OSVersion}) dotnet {Environment.Version}";
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        client.Timeout = TimeSpan.FromSeconds(5);

        return client;
    }

    public SPDXClient() : this(GetHttpClient())
    {
    }

    /// <summary>
    /// 获取缓存文件夹路径
    /// </summary>
    /// <returns></returns>
    private static string GetCacheFile(params string[] path)
    {
        var cache = Path.Combine([AppContext.BaseDirectory, ".cache", .. path]);
        var parent = Path.GetDirectoryName(cache);
        if (!Directory.Exists(parent) && !string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);

        return cache;
    }

    /// <summary>
    /// 获取许可证索引
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public async Task<LicensesIndex> GetLicenseIndexAsync()
    {
        await using var fs = await GetStreamAsync(
            GetCacheFile("index"),
            "https://spdx.org/licenses/licenses.json")
            .ConfigureAwait(false);

        return await JsonSerializer
            .DeserializeAsync<LicensesIndex>(fs)
            .ConfigureAwait(false)
            ?? throw new InvalidDataException("Cannot deserialize index.");
    }

    /// <summary>
    /// 获取许可证
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public async Task<LicenseDetails?> GetLicenseAsync(string licenseId)
    {
        var index = await GetLicenseIndexAsync()
            .ConfigureAwait(false);
        var license = index.Licenses
            ?.FirstOrDefault(i =>
                string.Equals(
                    i.LicenseId,
                    licenseId,
                    StringComparison.OrdinalIgnoreCase));

        if (license is null)
            return null;

        await using var fs = await GetStreamAsync(
            GetCacheFile("Licenses", license.LicenseId),
            license.DetailsUrl)
            .ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<LicenseDetails>(fs)
            .ConfigureAwait(false);
    }

    private async Task<FileStream> GetStreamAsync(string path, string url)
    {
        var etagPath = path + ".etag";
        if (File.Exists(path))
        {
            try
            {
                string etag = string.Empty;
                if (File.Exists(etagPath))
                    etag = File.ReadAllText(etagPath);

                // 尝试获取在线文件更新日期
                var onlineHead = await client
                    .SendAsync(
                        new(HttpMethod.Head, url))
                    .ConfigureAwait(false);

                string onlineEtag = onlineHead.Headers.ETag?.Tag.Trim('"') ?? string.Empty;

                if (string.Equals(etag, onlineEtag, StringComparison.OrdinalIgnoreCase))
                    return File.OpenRead(path);
            }
            catch (TaskCanceledException)
            {
                return File.OpenRead(path);
            }
        }

        // 缓存日期不同则更新缓存
        var response = await client.GetAsync(url).ConfigureAwait(false);
        await using var ns = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var fs = File.Create(path);
        File.WriteAllText(etagPath, response.Headers.ETag?.Tag.Trim('"'));
        await ns.CopyToAsync(fs).ConfigureAwait(false);
        fs.Seek(0, SeekOrigin.Begin);
        return fs;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!leaveOpen)
                    client.Dispose();
            }

            _disposedValue = true;
        }
    }

    // ~SPDXClient()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
