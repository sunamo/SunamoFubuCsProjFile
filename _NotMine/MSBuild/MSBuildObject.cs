namespace SunamoFubuCsProjFile._NotMine.MSBuild;

public class MSBuildObject
{
    public MSBuildObject(XmlElement elem)
    {
        Element = elem;
    }

    public XmlElement Element { get; }

    public string Condition
    {
        get => Element.GetAttribute("Condition");
        set
        {
            if (string.IsNullOrEmpty(value))
                Element.RemoveAttribute("Condition");
            else
                Element.SetAttribute("Condition", value);
        }
    }

    protected XmlElement AddChildElement(string name)
    {
        var e = Element.OwnerDocument.CreateElement(null, name, MSBuildProject.Schema);
        Element.AppendChild(e);
        return e;
    }
}
