namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;



public class ProjectRequest
{
    private static Type type = typeof(ProjectRequest);
    public readonly IList<string> Alterations = new List<string>();

    /// <summary>
    ///     For testing and extension projects
    /// </summary>
    public string OriginalProject;

    public string Version = DotNetVersion.V40;

    public ProjectRequest(string name, string template)
    {
        if (name == null) ThrowEx.IsNull("name");
        if (template == null) ThrowEx.IsNull("template");

        Name = name;
        Template = template;
    }

    public ProjectRequest(string name, string template, string originalProject)
        : this(name, template)
    {
        OriginalProject = originalProject;
    }

    public string Name { get; private set; }
    public string Template { get; private set; }

    public Substitutions Substitutions { get; } = new();
}
