namespace SunamoFubuCsProjFile._NotMine.MSBuild;

public class MSBuildPropertyGroup : MSBuildObject, MSBuildPropertySet
{
    private readonly Dictionary<string, MSBuildProperty> properties = new();

    public MSBuildPropertyGroup(MSBuildProject parent, XmlElement elem)
        : base(elem)
    {
        Parent = parent;
    }

    public MSBuildProject Parent { get; }

    public MSBuildProperty GetProperty(string name)
    {
        MSBuildProperty prop;
        if (properties.TryGetValue(name, out prop))
            return prop;
        var propElem = Element[name, MSBuildProject.Schema];
        if (propElem != null)
        {
            prop = new MSBuildProperty(propElem);
            properties[name] = prop;
            return prop;
        }

        return null;
    }

    public IEnumerable<MSBuildProperty> Properties
    {
        get
        {
            foreach (XmlNode node in Element.ChildNodes)
            {
                var pelem = node as XmlElement;
                if (pelem == null)
                    continue;
                MSBuildProperty prop;
                if (properties.TryGetValue(pelem.Name, out prop))
                {
                    yield return prop;
                }
                else
                {
                    prop = new MSBuildProperty(pelem);
                    properties[pelem.Name] = prop;
                    yield return prop;
                }
            }
        }
    }

    public MSBuildProperty SetPropertyValue(string name, string value, bool preserveExistingCase)
    {
        var prop = GetProperty(name);
        if (prop == null)
        {
            var pelem = AddChildElement(name);
            prop = new MSBuildProperty(pelem);
            properties[name] = prop;
            prop.Value = value;
        }
        else if (!preserveExistingCase || !string.Equals(value, prop.Value, StringComparison.OrdinalIgnoreCase))
        {
            prop.Value = value;
        }

        return prop;
    }

    public string GetPropertyValue(string name)
    {
        var prop = GetProperty(name);
        if (prop == null)
            return null;
        return prop.Value;
    }

    public bool RemoveProperty(string name)
    {
        var prop = GetProperty(name);
        if (prop != null)
        {
            properties.Remove(name);
            Element.RemoveChild(prop.Element);
            return true;
        }

        return false;
    }

    public void RemoveAllProperties()
    {
        var toDelete = new List<XmlNode>();
        foreach (XmlNode node in Element.ChildNodes)
            if (node is XmlElement)
                toDelete.Add(node);
        foreach (var node in toDelete)
            Element.RemoveChild(node);
        properties.Clear();
    }

    public void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propsToExclude)
    {
        foreach (var prop in baseGrp.Properties)
        {
            if (propsToExclude != null && propsToExclude.Contains(prop.Name))
                continue;
            var thisProp = GetProperty(prop.Name);
            if (thisProp != null && prop.Value.Equals(thisProp.Value, StringComparison.CurrentCultureIgnoreCase))
                RemoveProperty(prop.Name);
        }
    }

    public override string ToString()
    {
        var s = "[MSBuildPropertyGroup:";
        foreach (var prop in Properties)
            s += " " + prop.Name + "=" + prop.Value;
        return s + "]";
    }
}
