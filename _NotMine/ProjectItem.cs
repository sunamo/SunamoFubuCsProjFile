namespace SunamoFubuCsProjFile._NotMine;



public abstract class ProjectItem
{
    private string _include;

    protected ProjectItem(string name)
    {
        Name = name;
    }

    protected ProjectItem(string name, string include)
    {
        Name = name;
        Include = include;
    }

    public string Name { get; protected set; }

    public string Include
    {
        get => _include;
        set => _include = value.Replace('/', '\\');
    }

    protected MSBuildItem BuildItem { get; set; }

    public bool Matches(MSBuildItem item)
    {
        return item.Name == Name && item.Include == Include;
    }

    public virtual MSBuildItem Configure(MSBuildItemGroup group)
    {
        var item = group.Items.FirstOrDefault(Matches)
                   ?? group.AddNewItem(Name, Include);

        BuildItem = item;
        return item;
    }

    public virtual void Read(MSBuildItem item)
    {
        BuildItem = item;
        Include = item.Include;
    }

    public virtual void Save()
    {
        BuildItem.Include = Include;
    }

    protected bool Equals(ProjectItem other)
    {
        return string.Equals(Name, other.Name) && string.Equals(Include, other.Include);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ProjectItem)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name != null ? Name.GetHashCode() : 0) * 397 ^ (Include != null ? Include.GetHashCode() : 0);
        }
    }

    public override string ToString()
    {
        return string.Format("Item {0}: {1}", Name, Include);
    }
}
