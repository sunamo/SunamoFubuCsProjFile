namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;



public class Template
{
    public TemplateType Type { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Description { get; set; }

    public
#if ASYNC
        async Task<IEnumerable<Input>>
#else
IEnumerable<Input>
#endif
        Inputs()
    {
        return
#if ASYNC
            await
#endif
                Input.ReadFrom(Path);
    }
}
