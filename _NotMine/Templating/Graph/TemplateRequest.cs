namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;



public class TemplateRequest
{
    private readonly IList<ProjectRequest> _projects = new List<ProjectRequest>();
    private readonly IList<string> _templates = new List<string>();
    private readonly IList<ProjectRequest> _testingProjects = new List<ProjectRequest>();

    public Substitutions Substitutions { get; } = new();

    public string RootDirectory { get; set; }

    public IEnumerable<string> Templates
    {
        get => _templates;
        set
        {
            _templates.Clear();
            _templates.AddRange(value);
        }
    }

    // at the solution level
    public string SolutionName { get; set; }
    public string Version { get; set; }

    public IEnumerable<ProjectRequest> Projects
    {
        get => _projects;
        set
        {
            _projects.Clear();
            _projects.AddRange(value);
        }
    }

    public IEnumerable<ProjectRequest> TestingProjects
    {
        get => _testingProjects;
        set
        {
            _testingProjects.Clear();
            _testingProjects.AddRange(value);
        }
    }

    public void AddTemplate(string template)
    {
        _templates.Add(template);
    }

    public void AddProjectRequest(ProjectRequest request)
    {
        _projects.Add(request);
    }

    public void AddProjectRequest(string name, string template, Action<ProjectRequest> configuration = null)
    {
        var request = new ProjectRequest(name, template);
        if (configuration != null) configuration(request);

        _projects.Add(request);
    }

    public void AddTestingRequest(ProjectRequest request)
    {
        _testingProjects.Add(request);
    }

    public
#if ASYNC
        async Task<IEnumerable<MissingTemplate>>
#else
IEnumerable<MissingTemplate>
#endif
        Validate(ITemplateLibrary templates)
    {
        var solutionErrors =
#if ASYNC
            await
#endif
                templates.Validate(TemplateType.Solution, _templates.ToArray());
        var projectErrors =
#if ASYNC
            await
#endif
                templates.Validate(TemplateType.Project, _projects.Select(x => x.Template).ToArray());
        var alterationErrors =
#if ASYNC
            await
#endif
                templates.Validate(TemplateType.Alteration,
                    _projects.SelectMany(x => x.Alterations).ToArray());

        var testingErrors =
#if ASYNC
            await
#endif
                templates.Validate(TemplateType.Project,
                    _testingProjects.Select(x => x.Template).ToArray());

        var testingAlterationErrors =
#if ASYNC
            await
#endif
                templates.Validate(TemplateType.Alteration,
                    _testingProjects.SelectMany(x => x.Alterations).ToArray());


        return solutionErrors
            .Union(projectErrors)
            .Union(alterationErrors)
            .Union(testingErrors)
            .Union(testingAlterationErrors);
    }
}
