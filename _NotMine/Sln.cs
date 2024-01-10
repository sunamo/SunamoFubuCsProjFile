namespace SunamoFubuCsProjFile._NotMine;

[MarkedForTermination]
public class Sln
{
    private readonly IList<string> _postSolution = new List<string>();
    private readonly IList<CsprojFile> _projects = new List<CsprojFile>();

    public Sln(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; private set; }
    public IEnumerable<CsprojFile> Projects => _projects;
    public IEnumerable<string> PostSolutionConfiguration => _postSolution;

    public void AddProject(CsprojFile project)
    {
        _projects.Fill(project);
    }

    public void RegisterPostSolutionConfiguration(string projectGuid, string config)
    {
        var id = "{" + projectGuid + "}";
        _postSolution.Fill("\t\t{0}.{1}".ToFormat(id, config));
    }

    public void RegisterPostSolutionConfigurations(string projectGuid, params string[] configs)
    {
        configs.Each(config => RegisterPostSolutionConfiguration(projectGuid, config));
    }
}
