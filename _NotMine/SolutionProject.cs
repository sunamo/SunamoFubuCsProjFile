namespace SunamoFubuCsProjFile._NotMine;

public class SolutionProject
{
    public static string ProjectLineTemplate = "Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"";

    private IList<string> _directives = new List<string>();
    private Lazy<CsprojFile> _project;
    public Guid _projectGuid;

    public SolutionProject(CsprojFile csProjFile, string solutionDirectory)
    {
        _project = new Lazy<CsprojFile>(() => csProjFile);
        ProjectName = csProjFile.ProjectName;
        //FSXlf přemístit do SunamoExc. Xlf nemůže být includnutou do WithoutDep / notmine. To může být jedině SunamoExc
        RelativePath =
            Path.GetRelativePath(solutionDirectory,
                csProjFile.FileName); //csProjFile.FileName.PathRelativeTo(solutionDirectory);
        ProjectType = csProjFile.ProjectTypes().LastOrDefault();
        _projectGuid = csProjFile.ProjectGuid;
    }

    private string text = null;
    private string solutionDirectory = null;

    public SolutionProject(string text, string solutionDirectory)
    {
        this.text = text;
        this.solutionDirectory = solutionDirectory;
    }

    public
#if ASYNC
        async Task
#else
void
#endif
        Init()
    {
        var parts = text.ToDelimitedArray('=');
        ProjectType = Guid.Parse(parts.First().TextBetweenSquiggles());
        _projectGuid = Guid.Parse(parts.Last().TextBetweenSquiggles());

        var secondParts = parts.Last().ToDelimitedArray();
        ProjectName = secondParts.First().TextBetweenQuotes();
        RelativePath = secondParts.ElementAt(1).TextBetweenQuotes().Replace("\\", "/"); // Windows is forgiving


        _project = new Lazy<CsprojFile>(
            () =>
            {
                var filename = solutionDirectory.AppendPath(RelativePath);

                if (File.Exists(filename))
                {
                    var projFile =
                            CsprojFile.LoadFrom(filename)
#if ASYNC
                                .Result
#endif
                        ;
                    InitializeFromSolution(projFile, Solution);
                    return projFile;
                }

                var project = CsprojFile.CreateAtLocation(filename, ProjectName);
                project.ProjectGuid = _projectGuid;

                return project;
            }
        );
    }

    public Guid ProjectGuid => _projectGuid;

    public Guid ProjectType { get; private set; }

    public string ProjectName { get; private set; }

    public string RelativePath { get; set; }

    public CsprojFile Project => _project.Value;

    public Solution Solution { get; set; }

    public IList<ProjectSection> ProjectSections { get; } = new List<ProjectSection>();

    public ProjectDependenciesSection ProjectDependenciesSection =>
        ProjectSections.OfType<ProjectDependenciesSection>().FirstOrDefault();

    public static SolutionProject CreateNewAt(string solutionDirectory, string projectName)
    {
        var csProjFile = CsprojFile.CreateAtSolutionDirectory(projectName, solutionDirectory);
        return new SolutionProject(csProjFile, solutionDirectory);
    }


    private void InitializeFromSolution(CsprojFile projFile, Solution solution)
    {
        var tfsSourceControl =
            solution.Sections.FirstOrDefault(section => section.SectionName.Equals("TeamFoundationVersionControl"));
        if (tfsSourceControl != null) InitializeTfsSourceControlSettings(projFile, solution, tfsSourceControl);
    }

    private void InitializeTfsSourceControlSettings(CsprojFile projFile, Solution solution,
        GlobalSection tfsSourceControl)
    {
        var projUnique =
            tfsSourceControl.Properties.FirstOrDefault(item => item.EndsWith(Path.GetFileName(projFile.FileName)));
        if (projUnique == null) return;

        var index =
            Convert.ToInt32(projUnique.Substring("SccProjectUniqueName".Length,
                projUnique.IndexOf('=') - "SccProjectUniqueName".Length).Trim());

        projFile.SourceControlInformation = new SourceControlInformation(
            tfsSourceControl.Properties.First(item => item.StartsWith("SccProjectUniqueName" + index)).Split('=')[1]
                .Trim(),
            tfsSourceControl.Properties.First(item => item.StartsWith("SccProjectName" + index)).Split('=')[1]
                .Trim(),
            tfsSourceControl.Properties.First(item => item.StartsWith("SccLocalPath" + index)).Split('=')[1]
                .Trim());
    }

    public void Write(StringWriter writer)
    {
#if DEBUG
        if (ProjectName.Contains("cmd.web64"))
        {
        }
#endif

        writer.WriteLine(ProjectLineTemplate, ProjectType.ToString().ToUpper(), ProjectName,
            RelativePath.Replace('/', Path.DirectorySeparatorChar), _projectGuid.ToString().ToUpper());

        _directives.Each(x => writer.WriteLine(x));
        ProjectSections.Each(x => x.Write(writer));

        writer.WriteLine("EndProject");
    }

    public void ReadLine(string text)
    {
        _directives.Add(text);
    }

    public void AddProjectDependency(Guid projectGuid)
    {
        if (ProjectDependenciesSection == null) ProjectSections.Add(new ProjectDependenciesSection());

        // ReSharper disable once PossibleNullReferenceException, won't be null here, added above
        ProjectDependenciesSection.Add(projectGuid);
    }

    public void RemoveProjectDependency(Guid guid)
    {
        if (ProjectDependenciesSection == null) return;

        ProjectDependenciesSection.Remove(guid);
        if (ProjectDependenciesSection.Dependencies.Count == 0) ProjectSections.Remove(ProjectDependenciesSection);
    }

    public void RemoveAllProjectDependencies()
    {
        if (ProjectDependenciesSection == null) return;

        ProjectDependenciesSection.Clear();
        ProjectSections.Remove(ProjectDependenciesSection);
    }

    public override string ToString()
    {
        return string.Format("{0} : {1}", ProjectName, ProjectGuid.ToString("B").ToUpper());
    }
}

public static class CsprojFileExtensions
{
    public static string TextBetweenSquiggles(this string text)
    {
        var start = text.IndexOf("{");
        var end = text.IndexOf("}");

        return text.Substring(start + 1, end - start - 1);
    }

    public static string TextBetweenQuotes(this string text)
    {
        return text.Trim().TrimStart('"').TrimEnd('"');
    }
}
