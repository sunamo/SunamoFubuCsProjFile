namespace SunamoFubuCsProjFile;



public class SunamoFubuCsprojFileHelper
{
    private static Type type = typeof(SunamoFubuCsprojFileHelper);

    public static Solution slnOut = null;

    public static
#if ASYNC
        async Task<List<string>>
#else
List<string>
#endif
        GetProjectsInSlnFile(string item)
    {
        slnOut =
#if ASYNC
            await
#endif
                Solution.LoadFrom(item);

        var s = slnOut.Projects.Select(d => d.Project.FileName).ToList();

        for (var i = 0; i < s.Count; i++) s[i] = AbsoluteFromCombinePath(s[i]);

        return s;
    }

    /// <summary>
    /// Must be also here, to minimize shared code in SE.
    /// </summary>
    /// <param name="a"></param>
    public static string AbsoluteFromCombinePath(string a)
    {
        var r = Path.GetFullPath(new Uri(a).LocalPath);
        return r;
    }


    public static Guid GetProjectTypeFromSln(Solution sln, string v)
    {
        foreach (var item in sln.Projects)
            if (item.ProjectName == v)
                return item.ProjectType;
        return Guid.Empty;
    }

    public static Guid GetProjectIdFromSln(Solution sln, string v)
    {
        foreach (var item in sln.Projects)
            if (item.ProjectName == v)
                return item.ProjectGuid;
        return Guid.Empty;
    }
}
