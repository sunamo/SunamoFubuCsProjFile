namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;



public class TemplateLibrary : ITemplateLibrary
{
    public static readonly IFileSystem FileSystem = new FileSystem();
    public static readonly string Solution = "solution";
    public static readonly string Project = "project";
    public static readonly string Testing = "testing";
    public static readonly string Alteration = "alteration";

    public static readonly string DescriptionFile = "description.txt";

    private readonly Cache<TemplateType, string> _templateDirectories;

    private readonly string _templatesRoot;

    public TemplateGraph Graph = new();

    public TemplateLibrary(string templatesRoot)
    {
        _templatesRoot = templatesRoot;
        _templateDirectories = new Cache<TemplateType, string>(type =>
        {
            var directory = _templatesRoot.AppendPath(type.ToString().ToLowerInvariant());

            FileSystem.CreateDirectory(directory);

            return directory;
        });

        Enum.GetValues(typeof(TemplateType)).OfType<TemplateType>()
            .Each(x => _templateDirectories.FillDefault(x));

        var graphFile = templatesRoot.AppendPath(TemplateGraph.FILE);
        if (File.Exists(graphFile)) Graph = TemplateGraph.Read(graphFile);
    }

    public
#if ASYNC
        async Task<IEnumerable<Template>>
#else
IEnumerable<Template>
#endif
        All()
    {
        throw new Exception("Vyřeším později. nemůžu použít await na readTemplates protože předávám delegát");
        //           return _templateDirectories.GetAllKeys().SelectMany(
        //readTemplates);

        return new List<Template>();
    }

    public
#if ASYNC
        async Task<Template>
#else
Template
#endif
        Find(TemplateType type, string name)
    {
        var result = (
#if ASYNC
            await
#endif
                readTemplates(type)).FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
        return result;
    }

    public
#if ASYNC
        async Task<IEnumerable<Template>>
#else
IEnumerable<Template>
#endif
        Find(TemplateType type, IEnumerable<string> names)
    {
        var result = names.Select(
#if ASYNC
            async
#endif
                (x) =>
            {
                var result2 =
#if ASYNC
                    await
#endif

                        Find(type, x);
                return result2;
            });

#if ASYNC
        var joinedTasks = Task.WhenAll(result);
        return await joinedTasks;
#else
return result;
#endif
    }

    public
#if ASYNC
        async Task<IEnumerable<MissingTemplate>>
#else
IEnumerable<MissingTemplate>
#endif
        Validate(TemplateType type, params string[] names)
    {
        var templates =
#if ASYNC
            await
#endif
                readTemplates(type);

        var missingTemplates = new List<MissingTemplate>();

        foreach (var name in names)
        {
            if (templates.Any(x => x.Name.EqualsIgnoreCase(name))) continue;

            missingTemplates.Add(new MissingTemplate
            {
                Name = name,
                TemplateType = type,
                ValidChoices = templates.Select(x => x.Name).ToArray()
            });
        }

        return missingTemplates;
    }

    public static TemplateLibrary BuildClean(string root)
    {
        FileSystem.DeleteDirectory(root);
        FileSystem.CreateDirectory(root);

        return new TemplateLibrary(root);
    }

    public TemplateBuilder StartTemplate(TemplateType type, string name)
    {
        var directory = _templateDirectories[type].AppendPath(name);
        return new TemplateBuilder(directory);
    }

    private
#if ASYNC
        async Task<IEnumerable<Template>>
#else
IEnumerable<Template>
#endif
        readTemplates(TemplateType templateType)
    {
        var directory = _templateDirectories[templateType];

        var result = new List<Template>();

        foreach (var templateDirectory in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
        {
            var template = new Template
            {
                Name = Path.GetFileName(templateDirectory),
                Path = templateDirectory,
                Type = templateType
            };

            var descriptionFile = templateDirectory.AppendPath(DescriptionFile);
            if (FileSystem.FileExists(descriptionFile))
                template.Description =
#if ASYNC
                    await
#endif
                        FileSystem.ReadStringFromFile(descriptionFile);

            result.Add(template);
        }

        return result;
    }
}
