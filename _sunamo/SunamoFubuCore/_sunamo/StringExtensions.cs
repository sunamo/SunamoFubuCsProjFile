namespace SunamoFubuCsProjFile;


internal static class StringExtensions
{
    /// <summary>
    ///     If the path is rooted, just returns the path.  Otherwise,
    ///     combines root & path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    internal static string CombineToPath(this string path, string root)
    {
        if (Path.IsPathRooted(path)) return path;
        return Path.Combine(root, path);
    }
    internal static void IfNotNull(this string target, Action<string> continuation)
    {
        if (target != null) continuation(target);
    }
    internal static string ToFullPath(this string path)
    {
        return Path.GetFullPath(path);
    }
    /// <summary>
    ///     Retrieve the parent directory of a directory or file
    ///     Shortcut to Path.GetDirectoryName(path)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static string ParentDirectory(this string path)
    {
        return Path.GetDirectoryName(path.TrimEnd(Path.DirectorySeparatorChar));
    }
    /// <summary>
    ///     Equivalent of FileSystem.Combine( [Union of path, parts] )
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    internal static string AppendPath(this string path, params string[] parts)
    {
        var list = new List<string>
{
path
};
        list.AddRange(parts);
        return FileSystem.Combine(list.ToArray());
    }
    internal static string PathRelativeTo(this string path, string root)
    {
        var pathParts = path.getPathParts();
        var rootParts = root.getPathParts();
        var length = pathParts.Count > rootParts.Count ? rootParts.Count : pathParts.Count;
        for (var i = 0; i < length; i++)
            if (pathParts.First().Equals(rootParts.First(), StringComparison.OrdinalIgnoreCase))
            {
                pathParts.RemoveAt(0);
                rootParts.RemoveAt(0);
            }
            else
            {
                break;
            }
        for (var i = 0; i < rootParts.Count; i++) pathParts.Insert(0, "..");
        return pathParts.Count > 0 ? FileSystem.Combine(pathParts.ToArray()) : string.Empty;
    }
    internal static bool IsEmpty(this string stringValue)
    {
        return string.IsNullOrEmpty(stringValue);
    }
    internal static bool IsNotEmpty(this string stringValue)
    {
        return !string.IsNullOrEmpty(stringValue);
    }
    internal static void IsNotEmpty(this string stringValue, Action<string> action)
    {
        if (stringValue.IsNotEmpty())
            action(stringValue);
    }
    internal static bool ToBool(this string stringValue)
    {
        if (string.IsNullOrEmpty(stringValue)) return false;
        return bool.Parse(stringValue);
    }
    internal static string ToFormat(this string stringFormat, params Object[] args)
    {
        return string.Format(stringFormat, args);
    }
    /// <summary>
    ///     Performs a case-insensitive comparison of strings
    /// </summary>
    internal static bool EqualsIgnoreCase(this string thisString, string otherString)
    {
        return thisString.Equals(otherString, StringComparison.InvariantCultureIgnoreCase);
    }
    /// <summary>
    ///     Converts the string to Title Case
    /// </summary>
    internal static string Capitalize(this string stringValue)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stringValue);
    }
    /// <summary>
    ///     Formats a multi-line string for display on the web
    /// </summary>
    /// <param name="plainText"></param>
    internal static string ConvertCRLFToBreaks(this string plainText)
    {
        return new Regex("(\r\n|\n)").Replace(plainText, "<br/>");
    }
    /// <summary>
    ///     Returns a DateTime value parsed from the <paramref name="dateTimeValue" /> parameter.
    /// </summary>
    /// <param name="dateTimeValue">A valid, parseable DateTime value</param>
    /// <returns>The parsed DateTime value</returns>
    internal static DateTime ToDateTime(this string dateTimeValue)
    {
        return DateTime.Parse(dateTimeValue);
    }
    internal static string ToGmtFormattedDate(this DateTime date)
    {
        return date.ToString("yyyy'-'MM'-'dd hh':'mm':'ss tt 'GMT'");
    }
    internal static string[] ToDelimitedArray(this string content)
    {
        return content.ToDelimitedArray(',');
    }
    internal static string[] ToDelimitedArray(this string content, char delimiter)
    {
        var array = content.Split(delimiter);
        for (var i = 0; i < array.Length; i++) array[i] = array[i].Trim();
        return array;
    }
    internal static bool IsValidNumber(this string number)
    {
        return IsValidNumber(number, Thread.CurrentThread.CurrentCulture);
    }
    internal static bool IsValidNumber(this string number, CultureInfo culture)
    {
        var _validNumberPattern =
        @"^-?(?:\d+|\d{1,3}(?:"
        + culture.NumberFormat.NumberGroupSeparator +
        @"\d{3})+)?(?:\"
        + culture.NumberFormat.NumberDecimalSeparator +
        @"\d+)?$";
        return new Regex(_validNumberPattern, RegexOptions.ECMAScript).IsMatch(number);
    }
    internal static IList<string> getPathParts(this string path)
    {
        return path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    internal static string DirectoryPath(this string path)
    {
        return Path.GetDirectoryName(path);
    }
    /// <summary>
    ///     Reads text and returns an enumerable of strings for each line
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    internal static IEnumerable<string> ReadLines(this string text)
    {
        var reader = new StringReader(text);
        string line;
        while ((line = reader.ReadLine()) != null) yield return line;
    }
    /// <summary>
    ///     Reads text and calls back for each line of text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    internal static void ReadLines(this string text, Action<string> callback)
    {
        var reader = new StringReader(text);
        string line;
        while ((line = reader.ReadLine()) != null) callback(line);
    }
    /// <summary>
    ///     Just uses MD5 to create a repeatable hash
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    internal static string ToHash(this string text)
    {
        return MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text)).Select(b => b.ToString("x2")).Join("");
    }
    /// <summary>
    ///     Splits a camel cased string into seperate words delimitted by a space
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string SplitCamelCase(this string str)
    {
        return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
    }
    /// <summary>
    ///     Splits a pascal cased string into seperate words delimitted by a space
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string SplitPascalCase(this string str)
    {
        return str.SplitCamelCase();
    }
    internal static TEnum ToEnum<TEnum>(this string text) where TEnum : struct
    {
        var enumType = typeof(TEnum);
        if (!enumType.IsEnum) throw new ArgumentException("{0} is not an Enum".ToFormat(enumType.Name));
        return (TEnum)Enum.Parse(enumType, text, true);
    }
    /// <summary>
    ///     Wraps a string with parantheses.  Originally used to file escape file names when making command line calls
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    internal static string FileEscape(this string file)
    {
        return "\"{0}\"".ToFormat(file);
    }
}