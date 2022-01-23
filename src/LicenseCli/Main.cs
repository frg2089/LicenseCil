using static LicenseCli.Program;

var cache = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".cache");
if (File.Exists(cache))
    File.Delete(cache);
if (!Directory.Exists(cache))
    Directory.CreateDirectory(cache);

if (args.Length == 0 || args.Any(x => x.StartsWith("-h")))
{
    Help();
    return;
}

switch (args[0].ToLower())
{
    case "search":
    case "find":
        await SearchLicense(args[1]);
        break;
    case "list":
        await ListLicenses();
        break;
    case "create":
    case "new":
    case "use":
        args = args.Skip(1).ToArray();
        goto default;
    default:
        await UseLicense(
            args[0],
            args.Length > 1 ? args[1] : default,
            args.Any(x => x.StartsWith("-q")));
        break;
}