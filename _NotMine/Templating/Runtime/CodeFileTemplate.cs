namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class CodeFileTemplate : IProjectAlteration
{
    public const string CLASS = "%CLASS%";

    private static Type type = typeof(CodeFileTemplate);

    public CodeFileTemplate(string relativePath, string rawText)
    {
        if (Path.GetExtension(relativePath) != ".cs")
            ThrowEx.ArgumentOutOfRangeException("relativePath", "Relative Path must have the .cs extension");

        RelativePath = relativePath.Replace('\\', '/');

        RawText = rawText;
    }

    public string RelativePath { get; }

    public string RawText { get; }


    public
#if ASYNC
        async Task
#else
void
#endif
        Alter(CsprojFile file, ProjectPlan plan)
    {
        var includePath = plan.ApplySubstitutions(RelativePath);
        var filename = file.FileName.ParentDirectory().AppendPath(includePath);
        if (!filename.EndsWith(".cs")) filename = filename + ".cs";

        var text = plan.ApplySubstitutions(RawText, RelativePath);
        var fs = new FileSystem();


#if ASYNC
        await
#endif
            fs.WriteStringToFile(filename, text);

        file.Add<CodeFile>(includePath);
    }

    public static CodeFileTemplate Class(string relativePath)
    {
        var @class = Path.GetFileNameWithoutExtension(relativePath);

        var rawText = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(typeof(CodeFileTemplate), "Class.txt")
            .ReadAllText()
            .Replace(CLASS, @class);

        if (Path.GetExtension(relativePath) != ".cs") relativePath = relativePath + ".cs";

        return new CodeFileTemplate(relativePath, rawText);
    }

    public override string ToString()
    {
        return string.Format("Write and attach code file: {0}", RelativePath);
    }
}
