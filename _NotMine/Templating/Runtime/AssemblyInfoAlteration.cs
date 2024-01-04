namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class AssemblyInfoAlteration : IProjectAlteration
{
    public const string SourceFile = "assembly-info.txt";
    private readonly IEnumerable<string> _additions;
    public readonly string[] AssemblyInfoPath = { "Properties", "AssemblyInfo.cs" };

    public AssemblyInfoAlteration(params string[] additions)
    {
        _additions = additions;
    }

    /// <summary>
    /// Must be async even if there is no await due to IProjectAlteration
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
        var assemblyInfoPath = Path.Combine(AssemblyInfoPath);
        var codeFile = file.Find<CodeFile>(assemblyInfoPath) ?? file.Add<CodeFile>(assemblyInfoPath);

        var path = file.PathTo(codeFile);
        var parentDirectory = path.ParentDirectory();
        if (!Directory.Exists(parentDirectory)) Directory.CreateDirectory(parentDirectory);

        new FileSystem().AlterFlatFile(path, contents => Alter(contents, plan));
    }

    /// <summary>
    /// Must be async even if there is no await due to IProjectAlteration
    ///
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="plan"></param>
    /// <returns></returns>
    public
#if ASYNC
        async Task
#else
void
#endif
        Alter(List<string> contents, ProjectPlan plan)
    {
        _additions
            .Select(x => plan.ApplySubstitutions(x))
            .Where(x => !contents.Contains(x))
            .Each(contents.Add);
    }

    public override string ToString()
    {
        return string.Format("AssemblyInfo content:  {0}", _additions.Select(x => "'{0}'").Join("; "));
    }
}
