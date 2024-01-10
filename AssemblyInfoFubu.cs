namespace SunamoFubuCsProjFile;



public class AssemblyInfoFubu : CodeFile
{
    private readonly CodeFile _codeFile;
    private readonly FileSystem _fileSystem;
    private readonly CsprojFile _projFile;

    public AssemblyInfoFubu(CodeFile codeFile, CsprojFile projFile)
    {
        _codeFile = codeFile;
        _projFile = projFile;
        _fileSystem = new FileSystem();
        Initialize();
    }

    private string FullPath => _fileSystem.GetFullPath(Path.Combine(_projFile.ProjectDirectory, _codeFile.Include));

    private string[] Lines { get; set; }


    public Version AssemblyVersion { get; set; }

    public Version AssemblyFileVersion { get; set; }

    public string AssemblyTitle { get; set; }

    public string AssemblyDescription { get; set; }

    public string AssemblyConfiguration { get; set; }

    public string AssemblyCompany { get; set; }

    public string AssemblyProduct { get; set; }

    public string AssemblyCopyright { get; set; }

    public string AssemblyInformationalVersion { get; set; }

    public override void Save()
    {
        if (_fileSystem.FileExists(FullPath))
        {
            var result = new StringBuilder();

            if (AssemblyVersion != null) UpdateLine(Lines, "AssemblyVersion", AssemblyVersion.ToString());

            if (AssemblyFileVersion != null)
                UpdateLine(Lines, "AssemblyFileVersion", AssemblyFileVersion.ToString());

            UpdateLine(Lines, "AssemblyTitle", AssemblyTitle);
            UpdateLine(Lines, "AssemblyDescription", AssemblyDescription);
            UpdateLine(Lines, "AssemblyConfiguration", AssemblyConfiguration);
            UpdateLine(Lines, "AssemblyCompany", AssemblyCompany);
            UpdateLine(Lines, "AssemblyProduct", AssemblyProduct);
            UpdateLine(Lines, "AssemblyCopyright", AssemblyCopyright);
            UpdateLine(Lines, "AssemblyInformationalVersion", AssemblyInformationalVersion);

            Array.ForEach(Lines, s => result.AppendLine(s));
            _fileSystem.WriteStringToFile(FullPath, result.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }
    }

    public
#if ASYNC
        async Task
#else
void
#endif
        Initialize()
    {
        if (_fileSystem.FileExists(FullPath))
        {
            Lines = (
#if ASYNC
                await
#endif
                    _fileSystem.ReadStringFromFile(FullPath)).SplitOnNewLine();
            Parse("AssemblyVersion", value => AssemblyVersion = new Version(value.ExtractVersion()), Lines);
            Parse("AssemblyFileVersion", value => AssemblyFileVersion = new Version(value.ExtractVersion()), Lines);
            Parse("AssemblyTitle", value => AssemblyTitle = GetValueBetweenQuotes(value), Lines);
            Parse("AssemblyDescription", value => AssemblyDescription = GetValueBetweenQuotes(value), Lines);
            Parse("AssemblyConfiguration", value => AssemblyConfiguration = GetValueBetweenQuotes(value), Lines);
            Parse("AssemblyCompany", value => AssemblyCompany = GetValueBetweenQuotes(value), Lines);
            Parse("AssemblyProduct", value => AssemblyProduct = GetValueBetweenQuotes(value), Lines);
            Parse("AssemblyCopyright", value => AssemblyCopyright = GetValueBetweenQuotes(value), Lines);
            Parse("AssemblyInformationalVersion",
                value => AssemblyInformationalVersion = GetValueBetweenQuotes(value), Lines);
        }
    }

    private void UpdateLine(string[] lines, string property, string value)
    {
        for (var i = 0; i < lines.Length; i++)
            if (Match(property, lines[i]))
            {
                lines[i] = UpdateValueBetweenQuotes(lines[i], value);
                break;
            }
    }

    private void Parse(string property, Action<string> action, string[] lines)
    {
        var rawValue = lines.FirstOrDefault(line => Match(property, line));
        if (!string.IsNullOrWhiteSpace(rawValue)) action.Invoke(rawValue);
    }

    private static bool Match(string property, string line)
    {
        if (line.Trim().StartsWith(CSharpConsts.lc)) return false;

        return line.IndexOf(property, StringComparison.InvariantCultureIgnoreCase) > -1;
    }

    private string GetValueBetweenQuotes(string value)
    {
        var start = value.IndexOf('"') + 1;
        var end = value.IndexOf('"', start);

        return value.Substring(start, end - start);
    }

    private string UpdateValueBetweenQuotes(string line, string value)
    {
        var start = line.IndexOf('"') + 1;
        var end = line.IndexOf('"', start);

        return line.Substring(0, start) + value + line.Substring(end);
    }
}
