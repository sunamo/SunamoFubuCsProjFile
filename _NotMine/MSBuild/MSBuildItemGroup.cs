namespace SunamoFubuCsProjFile._NotMine.MSBuild;

public class MSBuildItemGroup : MSBuildObject
{
    private readonly MSBuildProject parent;

    public MSBuildItemGroup(MSBuildProject parent, XmlElement elem)
        : base(elem)
    {
        this.parent = parent;
    }

    public IEnumerable<MSBuildItem> Items
    {
        get
        {
            foreach (XmlNode node in Element.ChildNodes)
            {
                var elem = node as XmlElement;
                if (elem != null)
                    yield return parent.GetItem(elem);
            }
        }
    }

    public MSBuildItem AddNewItem(string name, string include)
    {
        var elem = AddChildElement(name);
        var it = parent.GetItem(elem);
        it.Include = include;
        return it;
    }
}
