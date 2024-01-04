namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;

public class ProjectCategory : DescribesItself
{
    public readonly IList<ProjectTemplate> Templates;

    public string Type;

    public ProjectCategory()
    {
        Templates = new List<ProjectTemplate>();
    }

    public void Describe(Description description)
    {
        description.Title = Type + " projects";
        description.ShortDescription = "Project templating options";
        description.AddList("Project Types", Templates);
    }

    public ProjectTemplate FindTemplate(string name)
    {
        return Templates.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
    }
}
