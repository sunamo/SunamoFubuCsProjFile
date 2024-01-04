namespace SunamoFubuCsProjFile._NotMine;

public class GlobalSection
{
    public GlobalSection(string declaration)
    {
        Declaration = declaration.Trim();
        LoadingOrder = declaration.Split('=').Last().Trim().ToEnum<SolutionLoading>();
        var start = declaration.IndexOf('(');
        var end = declaration.IndexOf(')');

        SectionName = declaration.Substring(start + 1, end - start - 1);
    }

    public string Declaration { get; }

    public string SectionName { get; }

    public IList<string> Properties { get; } = new List<string>();

    public SolutionLoading LoadingOrder { get; }

    public bool Empty => Properties == null || Properties.Count == 0;

    public void Read(string text)
    {
        Properties.Add(text.Trim());
    }

    public void Write(StringWriter writer)
    {
        writer.WriteLine("\t" + Declaration);
        Properties.Each(x => writer.WriteLine("\t\t" + x));
        writer.WriteLine("\t" + Solution.EndGlobalSection);
    }
}
