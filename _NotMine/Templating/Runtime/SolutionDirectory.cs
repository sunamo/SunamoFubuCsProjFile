namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class SolutionDirectory : ITemplateStep
{
    public SolutionDirectory(string relativePath)
    {
        RelativePath = relativePath.Replace("\\", "/");
    }

    public string RelativePath { get; }

    public void Alter(TemplatePlan plan)
    {
        new FileSystem().CreateDirectory(plan.Root, RelativePath);
    }

    public static IEnumerable<SolutionDirectory> PlanForDirectory(string root)
    {
        return Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
            .Select(dir => new SolutionDirectory(dir.PathRelativeTo(root)));
    }

    protected bool Equals(SolutionDirectory other)
    {
        return string.Equals(RelativePath, other.RelativePath);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SolutionDirectory)obj);
    }

    public override int GetHashCode()
    {
        return RelativePath != null ? RelativePath.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return string.Format("Create solution directory: {0}", RelativePath);
    }
}
