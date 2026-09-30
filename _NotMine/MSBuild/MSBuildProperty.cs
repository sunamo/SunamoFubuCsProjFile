namespace SunamoFubuCsProjFile;

public class MSBuildProperty : MSBuildObject
{
    public MSBuildProperty(XmlElement elem)
        : base(elem)
    {
    }

    public string Name => Element.Name;

    public string Value
    {
        get => Element.InnerXml;
        set => Element.InnerXml = value;
    }
}
