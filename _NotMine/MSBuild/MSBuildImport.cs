namespace SunamoFubuCsProjFile;

public class MSBuildImport : MSBuildObject
{
    public MSBuildImport(XmlElement elem) : base(elem)
    {
    }

    public string Project => Element.GetAttribute("Project");

    public string Name => Element.Name;
}
