namespace SunamoFubuCsProjFile;


internal class FileSet
{
    internal FileSet()
    {
        Include = "*.*";
        DeepSearch = true;
    }
    internal string Include { get; set; }
     internal string Exclude { get; set; }
    internal bool DeepSearch { get; set; }
    /// <summary>
    ///     Does a deep search in the folder
    /// </summary>
    /// <param name="include">Semicolon delimited list of search criteria to be included in the results</param>
    /// <param name="exclude">Semicolon delimited list of search criteria to be excluded in the results</param>
    /// <returns></returns>
    internal static FileSet Deep(string include, string exclude = null)
    {
        return new FileSet
        {
            DeepSearch = true,
            Exclude = exclude,
            Include = include
        };
    }
    /// <summary>
    ///     Does a shallow search in the immediate folder for files matching the path search
    /// </summary>
    /// <param name="include">Semicolon delimited list of search criteria to be included in the results</param>
    /// <param name="exclude">Semicolon delimited list of search criteria to be excluded in the results</param>
    /// <returns></returns>
    internal static FileSet Shallow(string include, string exclude = null)
    {
        return new FileSet
        {
            DeepSearch = false,
            Exclude = exclude,
            Include = include
        };
    }
    internal void AppendInclude(string include)
    {
        if (Include == "*.*") Include = string.Empty;
        if (Include.IsEmpty())
            Include = include;
        else
            Include += ";" + include;
    }
    internal void AppendExclude(string exclude)
    {
        if (Exclude.IsEmpty())
            Exclude = exclude;
        else
            Exclude += ";" + exclude;
    }
    internal IEnumerable<string> IncludedFilesFor(string path)
    {
        var directory = new DirectoryInfo(path);
        return directory.Exists
        ? getAllDistinctFiles(path, Include.IsEmpty() ? "*.*" : Include)
        : new string[0];
    }
    private IEnumerable<string> getAllDistinctFiles(string path, string pattern)
    {
        if (pattern.IsEmpty()) return new string[0];
        return pattern.Split(';').SelectMany(x =>
        {
            var fullPath = path;
            var dirParts = x.Replace("\\", "/").Split('/');
            var filePattern = x;
            if (dirParts.Length > 1)
            {
                var subFolder = dirParts.Take(dirParts.Length - 1).Join(Path.DirectorySeparatorChar.ToString());
                fullPath = Path.Combine(fullPath, subFolder);
                filePattern = dirParts.Last();
            }
            var directory = new DirectoryInfo(fullPath);
            var searchOption = DeepSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            try
            {
                return directory.Exists
        ? Directory.GetFiles(fullPath, filePattern, searchOption)
        : new string[0];
            }
            catch (DirectoryNotFoundException)
            {
                return new string[0];
            }
        }).Distinct();
    }
    internal IEnumerable<string> ExcludedFilesFor(string path)
    {
        return getAllDistinctFiles(path, Exclude);
    }
    internal static FileSet ForAssemblyNames(IEnumerable<string> assemblyNames)
    {
        return new FileSet
        {
            DeepSearch = false,
            Exclude = null,
            Include = assemblyNames.OrderBy(x => x).Select(x => "{0}.dll;{0}.exe".ToFormat(x)).Join(";")
        };
    }
    internal static FileSet ForAssemblyDebugFiles(IEnumerable<string> assemblyNames)
    {
        return new FileSet
        {
            DeepSearch = false,
            Exclude = null,
            Include = assemblyNames.OrderBy(x => x).Select(x => "{0}.pdb".ToFormat(x)).Join(";")
        };
    }
    internal bool Equals(FileSet other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(other.Include, Include) && Equals(other.Exclude, Exclude) &&
        other.DeepSearch.Equals(DeepSearch);
    }
    internal override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(FileSet)) return false;
        return Equals((FileSet)obj);
    }
    internal override int GetHashCode()
    {
        unchecked
        {
            var result = Include != null ? Include.GetHashCode() : 0;
            result = result * 397 ^ (Exclude != null ? Exclude.GetHashCode() : 0);
            result = result * 397 ^ DeepSearch.GetHashCode();
            return result;
        }
    }
    internal override string ToString()
    {
        return string.Format("Include: {0}, Exclude: {1}", Include, Exclude);
    }
    internal static FileSet Everything()
    {
        return new FileSet { DeepSearch = true, Include = "*.*" };
    }
}