namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class CopyFileToProject : IProjectAlteration
{
    private readonly string _relativePath;
    private readonly string _source;

    public CopyFileToProject(string relativePath, string source)
    {
        _relativePath = relativePath.Replace('\\', '/');
        _source = source;
    }

    public
#if ASYNC
        async Task
#else
void
#endif
        Alter(CsprojFile file, ProjectPlan plan)
    {
        var fileSystem = new FileSystem();
        var rawText =
#if ASYNC
            await
#endif
                fileSystem.ReadStringFromFile(_source);

        var templatedText = plan.ApplySubstitutions(rawText, _relativePath);

        var expectedPath = file.ProjectDirectory.AppendPath(_relativePath);


#if ASYNC
        await
#endif
            fileSystem.WriteStringToFile(expectedPath, templatedText);

        file.Add(new Content(_relativePath));
    }

    public override string ToString()
    {
        return string.Format("Copy {0} to {1}", _source, _relativePath);
    }
}
