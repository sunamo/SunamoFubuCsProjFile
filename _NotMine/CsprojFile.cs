namespace SunamoFubuCsProjFile._NotMine;



public static class DotNetVersion
{
    public static readonly string V40 = "v4.0";
    public static readonly string V45 = "v4.5";
}

public class CsprojFile
{
    /*
    <RootNamespace>MyProject</RootNamespace>
    <AssemblyName>MyProject</AssemblyName>
    */
    private const string PROJECTGUID = "ProjectGuid";
    private const string ROOT_NAMESPACE = "RootNamespace";
    private const string ASSEMBLY_NAME = "AssemblyName";

    public const string cProjectTypeGuids = "ProjectTypeGuids";
    public static readonly Guid ClassLibraryType = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
    public static readonly Guid WebSiteLibraryType = new("E24C65DC-7377-472B-9ABA-BC803B73C61A");
    public static readonly Guid VisualStudioSetupLibraryType = new("54435603-DBB4-11D2-8724-00A0C9A8B90C");

    private readonly Dictionary<string, ProjectItem> _projectItemCache = new();

    private AssemblyInfoFubu assemblyInfo;

    /// <summary>
    ///     before call must check A1 exists
    /// </summary>
    /// <param name="fileName"></param>
    public CsprojFile(string fileName) : this(fileName, MSBuildProject.LoadFromAsync(fileName)
#if ASYNC
            .Result
#endif
    )
    {
    }

    /// <summary>
    ///     before call must check A1 exists
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="project"></param>
    public CsprojFile(string fileName, MSBuildProject project)
    {
        FileName = fileName;
        BuildProject = project;
    }

    public bool IsValidXml => BuildProject != null && BuildProject.doc != null;

    public Guid ProjectGuid
    {
        get
        {
            var raw = BuildProject.PropertyGroups.Select(x => x.GetPropertyValue(PROJECTGUID))
                .FirstOrDefault(x => x.IsNotEmpty());

            return raw.IsEmpty() ? Guid.Empty : Guid.Parse(raw.TrimStart('{').TrimEnd('}'));
        }
        set
        {
            var group = BuildProject.PropertyGroups.FirstOrDefault(
                            x => x.Properties.Any(p => p.Name == PROJECTGUID))
                        ?? BuildProject.PropertyGroups.FirstOrDefault() ?? BuildProject.AddNewPropertyGroup(true);

            group.SetPropertyValue(PROJECTGUID, value.ToString().ToUpper(), true);
        }
    }

    public string AssemblyName
    {
        get
        {
            var group = BuildProject.PropertyGroups.FirstOrDefault(x =>
                            x.Properties.Any(p => p.Name == ASSEMBLY_NAME))
                        ?? BuildProject.PropertyGroups.FirstOrDefault() ?? BuildProject.AddNewPropertyGroup(true);

            return group.GetPropertyValue(ASSEMBLY_NAME);
        }
        set
        {
            var group = BuildProject.PropertyGroups.FirstOrDefault(x =>
                            x.Properties.Any(p => p.Name == ASSEMBLY_NAME))
                        ?? BuildProject.PropertyGroups.FirstOrDefault() ?? BuildProject.AddNewPropertyGroup(true);
            group.SetPropertyValue(ASSEMBLY_NAME, value, true);
        }
    }

    public string RootNamespace
    {
        get
        {
            var group = BuildProject.PropertyGroups.FirstOrDefault(x =>
                            x.Properties.Any(p => p.Name == ROOT_NAMESPACE))
                        ?? BuildProject.PropertyGroups.FirstOrDefault() ?? BuildProject.AddNewPropertyGroup(true);

            return group.GetPropertyValue(ROOT_NAMESPACE);
        }
        set
        {
            var group = BuildProject.PropertyGroups.FirstOrDefault(x =>
                            x.Properties.Any(p => p.Name == ROOT_NAMESPACE))
                        ?? BuildProject.PropertyGroups.FirstOrDefault() ?? BuildProject.AddNewPropertyGroup(true);
            group.SetPropertyValue(ROOT_NAMESPACE, value, true);
        }
    }

    public string ProjectName => Path.GetFileNameWithoutExtension(FileName);

    public MSBuildProject BuildProject { get; }

    public string FileName { get; }

    public string ProjectDirectory => FileName.ParentDirectory();

    public FrameworkName FrameworkName => BuildProject.FrameworkName;

    public string DotNetVersion
    {
        get
        {
            return BuildProject.PropertyGroups.Select(x => x.GetPropertyValue("TargetFrameworkVersion"))
                .FirstOrDefault(x => x.IsNotEmpty());
        }
        set
        {
            var group = BuildProject.PropertyGroups.FirstOrDefault(x =>
                            x.Properties.Any(p => p.Name == "TargetFrameworkVersion"))
                        ?? BuildProject.PropertyGroups.FirstOrDefault() ?? BuildProject.AddNewPropertyGroup(true);

            group.SetPropertyValue("TargetFrameworkVersion", value, true);
        }
    }

    public SourceControlInformation SourceControlInformation { get; set; }

    public AssemblyInfoFubu AssemblyInfo
    {
        get
        {
            if (assemblyInfo == null)
            {
                var codeFile = All<CodeFile>().FirstOrDefault(item => item.Include.EndsWith("AssemblyInfo.cs"));
                if (codeFile != null)
                {
                    assemblyInfo = new AssemblyInfoFubu(codeFile, this);
                    _projectItemCache.Add("AssemblyInfo+" + codeFile.Include, assemblyInfo);
                }
            }

            return assemblyInfo;
        }
    }

    public TargetFrameworkVersion TargetFrameworkVersion
    {
        get => BuildProject.GetGlobalPropertyGroup().GetPropertyValue("TargetFrameworkVersion");
        set => BuildProject.GetGlobalPropertyGroup().SetPropertyValue("TargetFrameworkVersion", value, false);
    }


    public string Platform
    {
        get => BuildProject.GetGlobalPropertyGroup().GetPropertyValue("Platform");
        set => BuildProject.GetGlobalPropertyGroup().SetPropertyValue("Platform", value, false);
    }

    /// <summary>
    ///     Creates a new CsprojFile in a folder relative to the solution folder
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="directory"></param>
    public static CsprojFile CreateAtSolutionDirectory(string assemblyName, string directory)
    {
        var fileName = directory.AppendPath(assemblyName).AppendPath(assemblyName) + ".csproj";
        var project = MSBuildProject.Create(assemblyName);
        return CreateCore(project, fileName);
    }

    /// <summary>
    ///     Creates a new class library project at the given filename
    ///     and assembly name
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="assemblyName"></param>
    public static CsprojFile CreateAtLocation(string fileName, string assemblyName)
    {
        return CreateCore(MSBuildProject.Create(assemblyName), fileName);
    }

    private static CsprojFile CreateCore(MSBuildProject project, string fileName)
    {
        var group = project.PropertyGroups.FirstOrDefault(x => x.Properties.Any(p => p.Name == PROJECTGUID)) ??
                    project.PropertyGroups.FirstOrDefault() ?? project.AddNewPropertyGroup(true);

        group.SetPropertyValue(PROJECTGUID, Guid.NewGuid().ToString().ToUpper(), true);

        var file = new CsprojFile(fileName, project);
        file.AssemblyName = file.RootNamespace = file.ProjectName;
        return file;
    }

    public void Add<T>(T item) where T : ProjectItem
    {
        var group = BuildProject.FindGroup(item.Matches) ??
                    BuildProject.FindGroup(x => x.Name == item.Name) ?? BuildProject.AddNewItemGroup();
        item.Configure(group);
    }

    public T Add<T>(string include) where T : ProjectItem, new()
    {
        var item = new T { Include = include };

        _projectItemCache.Remove(item.Include);
        _projectItemCache.Add(include, item);
        Add(item);

        return item;
    }

    public IEnumerable<T> All<T>() where T : ProjectItem, new()
    {
        var name = new T().Name;

        return BuildProject.GetAllItems(name).OrderBy(x => x.Include)
            .Select(item =>
            {
                T projectItem;
                if (_projectItemCache.ContainsKey(item.Include))
                {
                    projectItem = (T)_projectItemCache[item.Include];
                }
                else
                {
                    projectItem = new T();
                    projectItem.Read(item);
                    _projectItemCache.Add(item.Include, projectItem);
                }

                return projectItem;
            });
    }


    /// <summary>
    ///     Creates a new CsprojFile in a folder relative to the solution folder
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="directory"></param>
    public static
#if ASYNC
        async Task<CsprojFile>
#else
CsprojFile
#endif
        LoadFrom(string filename)
    {
        var project =
#if ASYNC
            await
#endif
                MSBuildProject.LoadFromAsync(filename);
        return new CsprojFile(filename, project);
    }

    public void Save()
    {
        Save(FileName);
    }

    public void Save(string file)
    {
        foreach (var item in _projectItemCache) item.Value.Save();

        BuildProject.Save(file);
    }

    public void RemovePropertyValues(string cProjectTypeGuids)
    {
        var l = BuildProject.PropertyGroups as IList<MSBuildPropertyGroup>;
        var il = BuildProject.PropertyGroups as IList;
        var ic = BuildProject.PropertyGroups as ICollection;
    }

    public IEnumerable<Guid> ProjectTypes()
    {
        var raws =
            BuildProject.PropertyGroups.Select(x => x.GetPropertyValue(cProjectTypeGuids))
                .Where(x => x.IsNotEmpty());

        if (raws.Any())
            foreach (var raw in raws)
                foreach (var guid in raw.Split(';'))
                    yield return Guid.Parse(guid.TrimStart('{').TrimEnd('}'));
        else
            yield return ClassLibraryType; // Class library
    }

    public void SetPropertyValue(string key, string value)
    {
        var msBuildPropertyGroup = BuildProject.AddNewPropertyGroup(false);
        msBuildPropertyGroup.SetPropertyValue(key, value, false);
    }

    public void CopyFileTo(string source, string relativePath)
    {
        var target = FileName.ParentDirectory().AppendPath(relativePath);
        new FileSystem().Copy(source, target);
    }

    public T Find<T>(string include) where T : ProjectItem, new()
    {
        return All<T>().FirstOrDefault(x => x.Include == include);
    }

    public string PathTo(CodeFile codeFile)
    {
        var path = codeFile.Include;
        if (SunamoFubuCore.Platform.IsUnix()) path = path.Replace('\\', Path.DirectorySeparatorChar);
        return FileName.ParentDirectory().AppendPath(path);
    }

    public void Remove<T>(string include) where T : ProjectItem, new()
    {
        var name = new T().Name;

        _projectItemCache.Remove(include);

        var element = BuildProject.GetAllItems(name).FirstOrDefault(x => x.Include == include);
        if (element != null) element.Remove();
    }

    public void Remove<T>(T item) where T : ProjectItem, new()
    {
        _projectItemCache.Remove(item.Include);

        var element = BuildProject.GetAllItems(item.Name).FirstOrDefault(x => x.Include == item.Include);
        if (element != null) element.Remove();
    }

    public override string ToString()
    {
        return string.Format("{0}", FileName);
    }
}
