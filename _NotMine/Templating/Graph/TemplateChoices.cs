namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;

/// <summary>
///     Only used to build out one ProjectRequest at a time
/// </summary>
public class TemplateChoices
{
    public readonly Cache<string, string> Inputs = new Cache<string, string>();
    public string Category;

    public IEnumerable<string> Options;
    public string ProjectName;
    public string ProjectType;
    public Cache<string, string> Selections = new Cache<string, string>();
}
