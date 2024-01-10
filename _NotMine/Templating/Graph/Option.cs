namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;

public class Option : DescribesItself
{
    public IList<string> Alterations = new List<string>();

    public string Description;
    public string Name;
    public string Url;

    public Option()
    {
    }

    public Option(string name, params string[] alterations)
    {
        Name = name;
        Alterations.AddRange(alterations);
    }

    public void Describe(Description description)
    {
        description.Title = Name;
        description.ShortDescription = Description;

        if (Url.IsNotEmpty()) description.Properties["Url"] = Url;
    }

    public Option DescribedAs(string description)
    {
        Description = description;
        return this;
    }
}
