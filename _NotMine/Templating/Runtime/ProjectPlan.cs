namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class ProjectPlan : ITemplateStep
{
    public const string NAMESPACE = "%NAMESPACE%";
    public const string ASSEMBLY_NAME = "%ASSEMBLY_NAME%";
    public const string SHORT_NAME = "%SHORT_NAME%";
    public const string PROJECT_PATH = "%PROJECT_PATH%";
    public const string PROJECT_FOLDER = "%PROJECT_FOLDER%";
    public const string RAKE_TASK_PREFIX = "%RAKE_TASK_PREFIX%";

    public static readonly string TemplateFile = "csproj.xml";

    private string _relativePath;

    public ProjectPlan(string projectName)
    {
        ProjectName = projectName;

        Substitutions.Set(ASSEMBLY_NAME, projectName);
        var shortName = projectName.Split('.').Last();
        Substitutions.Set(SHORT_NAME, shortName);

        Substitutions.Set(RAKE_TASK_PREFIX, shortName.ToLower());


        DotNetVersion = _NotMine.DotNetVersion.V40;
    }

    public Substitutions Substitutions { get; } = new();

    public IList<string> NugetDeclarations { get; } = new List<string>();

    public string ProjectTemplateFile { get; set; }

    public string ProjectName { get; }

    public IList<IProjectAlteration> Alterations { get; } = new List<IProjectAlteration>();

    public string DotNetVersion { get; set; }

    public void Alter(TemplatePlan plan)
    {
        // Hokey.
        Substitutions.Set(TemplatePlan.INSTRUCTIONS, plan.GetInstructions());

        plan.Logger.StartProject(Alterations.Count);
        plan.StartProject(this);

        Substitutions.Trace(plan.Logger);

        var reference = plan.Solution.FindProject(ProjectName);
        if (reference == null)
        {
            if (ProjectTemplateFile.IsEmpty())
            {
                plan.Logger.Trace("Creating project {0} from the default template", ProjectName);
                reference = plan.Solution.AddProject(ProjectName);
            }
            else
            {
                plan.Logger.Trace("Creating project {0} from template at {1}", ProjectName, ProjectTemplateFile);
                // Must be .Result. See ITemplateStep why
                reference = plan.Solution.AddProjectFromTemplate(ProjectName, ProjectTemplateFile)
#if ASYNC
                        .Result
#endif
                    ;
            }

            reference.Project.AssemblyName = reference.Project.RootNamespace = ProjectName;
            if (DotNetVersion != null) reference.Project.DotNetVersion = DotNetVersion;
        }

        var projectDirectory = reference.Project.ProjectDirectory;
        plan.FileSystem.CreateDirectory(projectDirectory);

        _relativePath = reference.Project.FileName.PathRelativeTo(plan.Root).Replace("\\", "/");
        Substitutions.Set(PROJECT_PATH, _relativePath);
        Substitutions.Set(PROJECT_FOLDER, _relativePath.Split('/').Reverse().Skip(1).Reverse().Join("/"));

        Alterations.Each(x =>
        {
            plan.Logger.TraceAlteration(ApplySubstitutions(x.ToString()));
            x.Alter(reference.Project, this);
        });


        Substitutions.WriteTo(projectDirectory.AppendPath(Substitutions.ConfigFile));

        plan.Logger.EndProject();
    }

    public void Add(IProjectAlteration alteration)
    {
        Alterations.Add(alteration);
    }

    public string ToNugetImportStatement()
    {
        return "{0}: {1}".ToFormat(ProjectName, NugetDeclarations.OrderBy(x => x).Join(", "));
    }

    public string ApplySubstitutions(string rawText, string relativePath = null)
    {
        return Substitutions.ApplySubstitutions(rawText, builder => writeNamespace(relativePath, builder));
    }

    public void ApplySubstitutions(string relativePath, StringBuilder builder)
    {
        Substitutions.ApplySubstitutions(builder);
        writeNamespace(relativePath, builder);
    }

    private void writeNamespace(string relativePath, StringBuilder builder)
    {
        if (relativePath.IsNotEmpty())
        {
            var ns = GetNamespace(relativePath, ProjectName);
            builder.Replace(NAMESPACE, ns);
        }
    }

    public static string GetNamespace(string relativePath, string projectName)
    {
        return relativePath
            .Split('/')
            .Reverse()
            .Skip(1)
            .Union(new[] { projectName })
            .Reverse()
            .Join(".");
    }

    public override string ToString()
    {
        return "Create or load project '{0}'".ToFormat(ProjectName);
    }
}
