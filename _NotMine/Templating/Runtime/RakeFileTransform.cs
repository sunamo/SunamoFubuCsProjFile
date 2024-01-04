namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class RakeFileTransform : ITemplateStep
{
    public static readonly string TargetFile = "rakefile";
    public static readonly string SourceFile = "rake.txt";
    private readonly string _text;

    public RakeFileTransform(string text)
    {
        _text = text;
    }

    public
        void
        Alter(TemplatePlan plan)
    {
        var lines = plan.ApplySubstitutions(_text).SplitOnNewLine();

        var rakeFile = FindFile(plan.Root);
        var fileSystem = new FileSystem();


        var list =
            fileSystem.FileExists(rakeFile)
                ?
                //must have .Result. RakeFileTransform.Alter should use await but in 10 other occurences they don't
                //A to to celé brutálně zesložiťuje
                fileSystem.ReadStringFromFile(rakeFile)

#if ASYNC
                    .Result
#endif

                    .ReadLines().ToList()
                : new List<string>();

        if (list.ContainsSequence(lines)) return;

        list.Add(string.Empty);
        list.AddRange(lines);

        fileSystem.WriteStringToFile(rakeFile, list.Join(Environment.NewLine));
    }

    public static string FindFile(string directory)
    {
        if (File.Exists(directory.AppendPath("rakefile.rb")))
            return directory.AppendPath("rakefile.rb").ToFullPath();

        return directory.AppendPath(TargetFile).ToFullPath();
    }

    public override string ToString()
    {
        return "Add content to the rakefile:";
    }
}
