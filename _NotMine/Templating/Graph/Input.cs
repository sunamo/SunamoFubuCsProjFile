namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;

public class Input
{
    public static readonly string File = "inputs.txt";
    private string text = null;

    public Input()
    {
    }

    public Input(string text)
    {
        this.text = text;
    }

    public
#if ASYNC
        async Task
#else
void
#endif
        Init()
    {
        var parts = text.ToDelimitedArray();
        if (parts.First().Contains("="))
        {
            var nameParts = parts.First().Split('=');
            Name = nameParts.First();
            Default = nameParts.Last();
        }
        else
        {
            Name = parts.First();
        }

        Description = parts.Last();
    }

    public string Name { get; set; }
    public string Default { get; set; }
    public string Description { get; set; }

    public static
#if ASYNC
        async Task<IEnumerable<Input>>
#else
IEnumerable<Input>
#endif
        ReadFrom(string directory)
    {
        var fileSystem = new FileSystem();
        var file = directory.AppendPath(File);
        if (!fileSystem.FileExists(file)) return Enumerable.Empty<Input>();

#if ASYNC
        var result = ReadFromFile(file);

        throw new Exception("Nev�m si rady");
        return null;
        //var joined = Task.WhenAll( result);
        //var r2 = await joined;
        //return r2;
#else
return ReadFromFile(file);
#endif
    }

    public static
#if ASYNC
        async Task<IEnumerable<Input>>
#else
IEnumerable<Input>
#endif
        ReadFromFile(string file)
    {
        var result =
            (
#if ASYNC
                await
#endif
                    new FileSystem().ReadStringFromFile(file)).ReadLines().Where(x => x.IsNotEmpty())
            .Select(x => new Input(x));

        return result;
    }
}
