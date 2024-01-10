namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class ProjectDirectory : IProjectAlteration
{
    public ProjectDirectory(string relativePath)
    {
        RelativePath = relativePath.Replace("\\", "/");
    }

    public string RelativePath { get; }

    /// <summary>
    /// Must be async due to IProjectAlteration
    /// </summary>
    /// <param name="file"></param>
    /// <param name="plan"></param>
    /// <returns></returns>
    public
#if ASYNC
        async Task
#else
void
#endif
        Alter(CsprojFile file, ProjectPlan plan)
    {
        TemplateLibrary.FileSystem.CreateDirectory(file.ProjectDirectory.AppendPath(RelativePath));
    }

    protected bool Equals(ProjectDirectory other)
    {
        return string.Equals(RelativePath, other.RelativePath);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ProjectDirectory)obj);
    }

    public override int GetHashCode()
    {
        return RelativePath != null ? RelativePath.GetHashCode() : 0;
    }

    public static IEnumerable<ProjectDirectory> PlanForDirectory(string root)
    {
        return Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
            .Select(dir => new ProjectDirectory(dir.PathRelativeTo(root)));
    }

    public override string ToString()
    {
        return string.Format("Create folder {0}", RelativePath);
    }
}
