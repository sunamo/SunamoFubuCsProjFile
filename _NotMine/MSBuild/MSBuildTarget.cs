namespace SunamoFubuCsProjFile;

public class MSBuildTarget : MSBuildObject
{
    public MSBuildTarget(XmlElement elem)
        : base(elem)
    {
    }

    public string Name => Element.GetAttribute("Name");
}
