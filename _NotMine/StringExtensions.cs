namespace SunamoFubuCsProjFile._NotMine;

public static class StringExtensions
{
    private static readonly string[] Splitters = { Consts.rn, "\n" };

    public static string[] SplitOnNewLine(this string value)
    {
        return value.Split(Splitters, StringSplitOptions.None);
    }

    public static string CanonicalPath(this string path)
    {
        return path.ToFullPath().ToLower().Replace("\\", "/");
    }

    public static bool ContainsSequence(this List<string> list, IEnumerable<string> lines)
    {
        var index = list.IndexOf(lines.First());
        if (index == -1) return false;

        if (lines.Count() == 1) return true;

        var i = 0;
        foreach (var line in lines)
        {
            if (list.Count <= index + i) return false;

            if (list[index + i] != line) return false;

            i++;
        }

        return true;
    }

    public static string ExtractVersion(this string source)
    {
        var result = new StringBuilder();

        for (var i = 0; i < source.Length; i++)
        {
            var value = source[i];

            if (!char.IsDigit(value) && value != '.' && result.Length > 0)
                break;

            if (char.IsDigit(value) || value == '.' && result.Length > 0) result.Append(value);
        }

        return result.ToString().TrimEnd('.');
    }

    public static bool Contains(this string source, string value, StringComparison comparison)
    {
        switch (comparison)
        {
            case StringComparison.CurrentCultureIgnoreCase:
            case StringComparison.InvariantCultureIgnoreCase:
            case StringComparison.OrdinalIgnoreCase:
                if (source == null) return false;

                if (value == null) return false;

                return source.ToLower().Contains(value.ToLower());
            default:
                return source.Contains(value);
        }
    }
}
