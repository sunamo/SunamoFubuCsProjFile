namespace SunamoFubuCsProjFile._NotMine.Templating;



public class TextFile
{
    public static readonly IFileSystem FileSystem = new FileSystem();

    public TextFile(string path, string relativePath)
    {
        Path = path;
        RelativePath = relativePath.Replace('\\', '/');
    }

    public string RelativePath { get; }

    public string Path { get; }

    public
#if ASYNC
        async Task<string>
#else
string
#endif
        ReadAll()
    {
        return
#if ASYNC
            await
#endif
                FileSystem.ReadStringFromFile(Path);
    }

    public
#if ASYNC
        async Task<IEnumerable<string>>
#else
IEnumerable<string>
#endif
        ReadLines()
    {
        return (
#if ASYNC
            await
#endif
                ReadAll()).Trim().SplitOnNewLine();
    }
}
