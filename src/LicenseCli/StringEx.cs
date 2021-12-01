namespace LicenseCli;

internal static class StringEx
{
    public static string PadCenter(this string? @this, int totalWidth, char paddingChar = ' ')
    {
        if (string.IsNullOrEmpty(@this))
            return string.Empty;
        var space = totalWidth - @this.Length;
        var left = (space / 2) + @this.Length;
        return @this.PadLeft(left, paddingChar).PadRight(space + @this.Length, paddingChar);
    }
}
