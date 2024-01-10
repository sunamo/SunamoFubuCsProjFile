namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;

public class TemplateGraph
{
    public static readonly string FILE = "Templates.xml";

    private static Type type = typeof(TemplateGraph);


    private readonly IList<ProjectCategory> _categories = new List<ProjectCategory>();

    public static TemplateGraph Read(string file)
    {
        var document = new XmlDocument();
        document.Load(file);

        var graph = new TemplateGraph();

        foreach (XmlElement element in document.DocumentElement.SelectNodes("category"))
        {
            var category = new ProjectCategory { Type = element.GetAttribute("type") };
            foreach (XmlElement projectElement in element.SelectNodes("project"))
                category.Templates.Add(projectElement.BuildProjectTemplate());

            graph._categories.Add(category);
        }


        return graph;
    }

    public void AddCategory(ProjectCategory category)
    {
        _categories.Add(category);
    }

    public ProjectCategory FindCategory(string category)
    {
        return _categories.FirstOrDefault(x => x.Type.EqualsIgnoreCase(category));
    }

    public ProjectRequest BuildProjectRequest(TemplateChoices choices)
    {
        if (choices.Category.IsEmpty()) ThrowEx.Custom("Category is required");
        if (choices.ProjectName.IsEmpty()) ThrowEx.Custom("ProjectName is required");

        var category = FindCategory(choices.Category);
        if (category == null) ThrowEx.Custom("Category '{0}' is unknown".ToFormat(choices.Category));

        var project = category.FindTemplate(choices.ProjectType);
        if (project == null)
            ThrowEx.Custom(
                "ProjectTemplate '{0}' for category {1} is unknown".ToFormat(choices.ProjectType,
                    choices.Category));

        return project.BuildProjectRequest(choices);
    }

    public ProjectCategory AddCategory(string categoryName)
    {
        var category = new ProjectCategory { Type = categoryName };

        _categories.Add(category);

        return category;
    }
}
