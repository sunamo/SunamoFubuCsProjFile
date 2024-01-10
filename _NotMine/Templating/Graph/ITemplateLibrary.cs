namespace SunamoFubuCsProjFile._NotMine.Templating.Graph;



public interface ITemplateLibrary
{
#if ASYNC
    Task<IEnumerable<Template>>
#else
IEnumerable<Template>
#endif
        All();
#if ASYNC
    Task<Template>
#else
Template
#endif
        Find(TemplateType type, string name);

#if ASYNC
    Task<IEnumerable<Template>>
#else
IEnumerable<Template>
#endif
        Find(TemplateType type, IEnumerable<string> names);

#if ASYNC
    Task<IEnumerable<MissingTemplate>>
#else
IEnumerable<MissingTemplate>
#endif
        Validate(TemplateType type, params string[] names);
}

public class MissingTemplate
{
    public string Name { get; set; }
    public TemplateType TemplateType { get; set; }
    public string[] ValidChoices { get; set; }

    public override string ToString()
    {
        var validChoiceString = ValidChoices.Select(x => "'{0}'".ToFormat(x)).Join(", ");
        return "Unknown {0} template '{1}', valid choices are {2}"
            .ToFormat(TemplateType.ToString(), Name, validChoiceString);
    }
}
