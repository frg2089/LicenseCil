using DotMake.CommandLine;

using LicenseCli;

return args switch
{
    { Length: not 0 } => Cli.Run<RootCommand>(args),
    _ => Cli.Run<RootCommand>("--help")
};
