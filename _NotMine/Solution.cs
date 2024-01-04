namespace SunamoFubuCsProjFile._NotMine;



public class Solution
{
    private const string Global = "Global";
    private const string EndGlobal = "EndGlobal";
    public const string EndGlobalSection = "EndGlobalSection";
    public const string EndProjectSection = "EndProjectSection";
    private const string SolutionConfigurationPlatforms = "SolutionConfigurationPlatforms";
    private const string ProjectConfigurationPlatforms = "ProjectConfigurationPlatforms";

    public static readonly Guid SolutionFolderId = new("2150E333-8FDC-42A3-9474-1A3956D46DE8");

    public static readonly string VS2010 = "VS2010";
    public static readonly string VS2012 = "VS2012";
    public static readonly string VS2013 = "VS2013";
    public static readonly string DefaultVersion = VS2010;

    private static readonly Cache<string, string[]> _versionLines = new Cache<string, string[]>();

    private static Type type = typeof(Solution);

    private readonly IList<string> _globals = new List<string>();
    protected readonly IList<string> _header = new List<string>();
    private readonly IList<SolutionProject> _projects = new List<SolutionProject>();

    static Solution()
    {
        _versionLines[VS2010] = new[]
            { "Microsoft Visual Studio Solution File, Format Version 11.00", "# Visual Studio 2010" };
        _versionLines[VS2012] = new[]
            { "Microsoft Visual Studio Solution File, Format Version 12.00", "# Visual Studio 2012" };
        _versionLines[VS2013] = new[]
        {
            "Microsoft Visual Studio Solution File, Format Version 12.00", "# Visual Studio 2013",
            "VisualStudioVersion = 12.0.21005.1", "MinimumVisualStudioVersion = 10.0.40219.1"
        };
    }

    private Solution(string filename, string text)
    {
        Filename = filename;
        var items = text.SplitOnNewLine();
        var reader = new SolutionReader(this);

#if DEBUG
        if (filename == @"E:\vs\Projects\sunamoWithoutDep\sunamoWithoutDep.sln")
        {
        }
#endif

        // zde je problém, .Each v Solution.cs (řádek 68) chce jen Action. avšak potřebuji Func aby mi to vrátilo task. Vkládaní do asynchronních lambd nejde. Jistě řešení to bude mít, ale já jako zelenáč v async to zatím nesvedu.
        //

        items.Each(reader.Read);

        //            items.Each(
        //#if ASYNC
        //    async () => await reader.Read
        //#else
        //reader.Read
        //#endif
        //);
    }

    /// <summary>
    ///     Specify the VS.Net version.  At this time, the valid options are
    ///     "VS2010" or "VS2012" or "VS2013"
    /// </summary>
    public string Version { get; set; }

    public string Filename { get; }

    public IList<GlobalSection> Sections { get; } = new List<GlobalSection>();

    public IEnumerable<string> Globals => _globals;

    public IEnumerable<SolutionProject> Projects => _projects;

    public string ParentDirectory => Filename.ParentDirectory();

    public string Name => Path.GetFileNameWithoutExtension(Filename);

    /// <summary>
    ///     Creates a new empty Solution file with the supplied name that
    ///     will be written to the directory given upon calling Save()
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="name"></param>
    public static Solution CreateNew(string directory, string name)
    {
        var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Solution), "Solution.txt")
            .ReadAllText();

        var filename = directory.AppendPath(name);
        if (Path.GetExtension(filename) != ".sln") filename = filename + ".sln";

        return new Solution(filename, text)
        {
            Version = DefaultVersion
        };
    }

    /// <summary>
    ///     Loads an existing solution from a file
    /// </summary>
    /// <param name="filename"></param>
    public static
#if ASYNC
        async Task<Solution>
#else
Solution
#endif
        LoadFrom(string filename)
    {
#if DEBUG
        if (filename == @"E:\vs\Projects\sunamoWithoutDep\sunamoWithoutDep.sln")
        {
        }
#endif

        var text =
#if ASYNC
            await
#endif
                new FileSystem().ReadStringFromFile(filename);
        return new Solution(filename, text);
    }

    public IEnumerable<BuildConfiguration> Configurations()
    {
        var section = FindSection(SolutionConfigurationPlatforms);
        return section == null
            ? Enumerable.Empty<BuildConfiguration>()
            : section.Properties.Select(x => new BuildConfiguration(x));
    }

    public GlobalSection FindSection(string name)
    {
        return Sections.FirstOrDefault(x => x.SectionName == name);
    }

    /// <summary>
    ///     Save the solution to the known file location
    /// </summary>
    /// <param name="saveProjects">Whether the solution should call Save on all of its child projects, too.</param>
    public void Save(bool saveProjects = false)
    {
        Save(Filename, saveProjects);
    }

    /// <summary>
    ///     Save the solution to a different file
    ///     saveProjects = false is there extreme important! otherwise saving folders from root sln to subfolder of project of
    ///     same name!
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="saveProjects">Whether the solution should call Save on all of its child projects, too.</param>
    public void Save(string filename, bool saveProjects = false)
    {
        calculateProjectConfigurationPlatforms();

        var writer = new StringWriter();

        EnsureHeaders();
        _header.Each(x => writer.WriteLine(x));

        _projects.Each(x => x.Write(writer));

        writer.WriteLine(Global);

        Sections.Each(x => x.Write(writer));

        writer.WriteLine(EndGlobal);

        new FileSystem().WriteStringToFile(filename, writer.ToString());

        if (saveProjects) _projects.Each(x => x.Project.Save());
    }

    private void EnsureHeaders()
    {
        if (_header.Count == 0)
        {
            _header.Add(string.Empty); // Visual studio project always start with a blank line.
            _versionLines.ToDictionary()[Version ?? DefaultVersion].Each(_header.Add);
        }
    }

    private void calculateProjectConfigurationPlatforms()
    {
        var section = FindSection(ProjectConfigurationPlatforms);
        if (section == null)
        {
            section = new GlobalSection("GlobalSection(ProjectConfigurationPlatforms) = postSolution");
            Sections.Add(section);
        }

        section.Properties.Clear();
        var configurations = Configurations().ToArray();

        _projects.Where(x => x.ProjectName != "Solution Items").Each(proj =>
        {
            configurations.Each(config => config.WriteProjectConfiguration(proj, section));
        });

        if (section.Empty) Sections.Remove(section);
    }

    /// <summary>
    ///     Attaches an existing project of this name to the solution or
    ///     creates a new project based on a Class Library and attaches
    ///     to the solution
    /// </summary>
    /// <param name="projectName"></param>
    public SolutionProject AddProject(string projectName)
    {
        return AddProject(ParentDirectory, projectName);
    }

    /// <summary>
    ///     Attaches an existing project of this name to the solution or
    ///     creates a new project based on a Class Library and attaches
    ///     to the solution
    /// </summary>
    /// <param name="solutionFolder"></param>
    /// <param name="projectName"></param>
    public SolutionProject AddProject(string solutionFolder, string projectName)
    {
        var existing = FindProject(projectName);
        if (existing != null) return existing;

        var reference = SolutionProject.CreateNewAt(ParentDirectory, projectName);
        _projects.Add(reference);

        return reference;
    }

    /// <summary>
    ///     Adds an existing project
    /// </summary>
    /// <param name="project"></param>
    public void AddProject(CsprojFile project)
    {
        AddProject(ParentDirectory, project);
    }

    /// <summary>
    ///     Adds an existing project to the given <paramref name="solutionDirectory" />.
    /// </summary>
    /// <param name="solutionDirectory"></param>
    /// <param name="project"></param>
    public void AddProject(string solutionDirectory, CsprojFile project)
    {
        var existing = FindProject(project.ProjectName);
        if (existing != null) return;

        var reference = new SolutionProject(project, solutionDirectory);
        // This is needed for have in sln when add missing project to have
        // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "desktop2", "desktop2\desktop2.csproj", "{3F15EC21-5242-495C-A4E5-ED3C21415CFB}"
        // and not
        // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "desktop2", "sunamo\desktop2\desktop2.csproj", "{00000000-0000-0000-0000-000000000000}"
        reference._projectGuid = Guid.NewGuid();
        _projects.Add(reference);
    }

    /// <summary>
    ///     Adds a new project based on the supplied template file
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="templateFile"></param>
    public
#if ASYNC
        async Task<SolutionProject>
#else
SolutionProject
#endif
        AddProjectFromTemplate(string projectName, string templateFile)
    {
        var existing = FindProject(projectName);
        if (existing != null)
            ThrowEx.ArgumentOutOfRangeException("projectName",
                "Project with this name ({0}) already exists in the solution".ToFormat(projectName));


        var project =
#if ASYNC
            await
#endif
                MSBuildProject.CreateFromFile(projectName, templateFile);
        var csProjFile = new CsprojFile(ParentDirectory.AppendPath(projectName, projectName + ".csproj"), project);
        csProjFile.ProjectGuid = Guid.NewGuid();

        var reference = new SolutionProject(csProjFile, ParentDirectory);
        _projects.Add(reference);

        return reference;
    }

    public void RemoveProject(CsprojFile project)
    {
        var existing = FindProject(project.ProjectName);
        if (existing == null) return;

        _projects.Remove(existing);
    }

    /// <summary>
    ///     Access an attached project by name
    /// </summary>
    /// <param name="projectName"></param>
    public SolutionProject FindProject(string projectName)
    {
        return _projects.FirstOrDefault(x => x.ProjectName == projectName);
    }

    public override string ToString()
    {
        return string.Format("{0}", Filename);
    }

    public class SolutionReader
    {
        private static readonly HashSet<string> ignoredLibraryTypes = new()
        {
            SolutionFolderId.ToString("B"),
            CsprojFile.VisualStudioSetupLibraryType.ToString("B"),
            CsprojFile.WebSiteLibraryType.ToString("B")
        };

        private readonly Solution _parent;
        private ProjectSection _projectSection;
        private Action<string> _read;
        private GlobalSection _section;
        private SolutionProject _solutionProject;

        public SolutionReader(Solution parent)
        {
            _parent = parent;

            _read = normalRead;
        }

        private
            void lookForGlobalSection(string text)
        {
            text = text.Trim();
            if (text.Trim().StartsWith("GlobalSection"))
            {
                _section = new GlobalSection(text);
                _parent.Sections.Add(_section);
                _read = readSection;
            }
        }

        private
            void
            lookForProjectSection(string text)
        {
            text = text.Trim();
            if (text.Trim().StartsWith("ProjectSection"))
            {
                _projectSection = text.Trim().StartsWith("ProjectSection(ProjectDependencies)")
                    ? new ProjectDependenciesSection(text)
                    : new ProjectSection(text);
                _solutionProject.ProjectSections.Add(_projectSection);
                _read = readProjectSection;
            }
        }

        private
            void
            readSection(string text)
        {
            if (text.Trim() == EndGlobalSection)
                _read = lookForGlobalSection;
            else
                _section.Read(text);
        }

        private
            void
            readProjectSection(string text)
        {
            if (text.Trim() == EndProjectSection)
                _read = readProject;
            else
                _projectSection.Read(text);
        }

        private
            void
            readProject(string text)
        {
            if (text.Trim().StartsWith("EndProject"))
            {
                _read = normalRead;
            }
            else
            {
                if (text.Trim().StartsWith("ProjectSection"))
                    lookForProjectSection(text);
                else
                    _solutionProject.ReadLine(text);
            }
        }

        private
            void
            normalRead(string text)
        {
            if (text.StartsWith(Global))
            {
                _read = lookForGlobalSection;
            }
            else if (text.StartsWith("ProjectSection"))
            {
                _read = lookForProjectSection;
            }
            else if (IncludeAsProject(text))
            {
                _solutionProject = new SolutionProject(text, _parent.Filename.ParentDirectory());

                // nechat, .Each v Solution.cs (řádek 68) chce jen Action. avšak potřebuji Func aby mi to vrátilo task. Vkládaní do asynchronních lambd nejde. Jistě řešení to bude mít, ale já jako zelenáč v async to zatím nesvedu.
                _solutionProject.Init();
                _solutionProject.Solution = _parent;
                _parent._projects.Add(_solutionProject);
                _read = readProject;
            }
            else
            {
                _parent._header.Add(text);
                if (_parent.Version.IsEmpty())
                    foreach (var versionLine in _versionLines.ToDictionary())
                        if (text.Trim() == versionLine.Value[1])
                            _parent.Version = versionLine.Key;
            }
        }

        public static bool IncludeAsProject(string text)
        {
            return text.StartsWith("Project") && !ignoredLibraryTypes.Any(item =>
                text.Contains(item, StringComparison.InvariantCultureIgnoreCase));
        }

        public
            void
            Read(string text)
        {
            _read(text);
        }
    }
}
