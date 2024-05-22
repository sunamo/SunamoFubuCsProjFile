namespace SunamoFubuCsProjFile;


internal class Description
{
    internal Description()
    {
        BulletLists = new List<BulletList>();
    }
    internal Type TargetType { get; set; }
    internal string Title { get; set; }
    internal string ShortDescription { get; set; }
    internal string LongDescription { get; set; }
    internal Cache<string, string> Properties { get; } = new Cache<string, string>();
    internal Cache<string, Description> Children { get; } = new Cache<string, Description>();
    internal IList<BulletList> BulletLists { get; }
    internal bool HasExplicitShortDescription()
    {
        if (ShortDescription.IsEmpty()) return false;
        if (TargetType == null) return true;
        return ShortDescription != TargetType.FullName;
    }
    internal bool HasMoreThanTitle()
    {
        return HasExplicitShortDescription() || BulletLists.Any() || Children.Any() || Properties.Any();
    }
    internal BulletList AddList(string name, IEnumerable objects)
    {
        var list = new BulletList
        {
            Name = name
        };
        BulletLists.Add(list);
        objects.Each(x =>
        {
            var desc = For(x);
            list.Children.Add(desc);
        });
        return list;
    }
    internal static Description For(object target)
    {
        var type = target.GetType();
        var description = new Description
        {
            TargetType = target.GetType(),
            Title = type.Name,
            ShortDescription = target.ToString()
        };
        type.ForAttribute<DescriptionAttribute>(x => description.ShortDescription = x.Description);
        type.ForAttribute<TitleAttribute>(x => description.Title = x.Title);
        (target as DescribesItself).CallIfNotNull(x => x.Describe(description));
        return description;
    }
    internal static bool HasExplicitDescription(Type type)
    {
        return type.CanBeCastTo<DescribesItself>() || type.HasAttribute<DescriptionAttribute>() ||
        type.HasAttribute<TitleAttribute>();
    }
    internal override string ToString()
    {
        return string.Format("{0}: {1}", Title, ShortDescription);
    }
    internal void AcceptVisitor(IDescriptionVisitor visitor)
    {
        visitor.Start(this);
        BulletLists.Each(x => x.AcceptVisitor(visitor));
        visitor.End();
    }
    internal bool IsMultiLevel()
    {
        return BulletLists.Any() || Children.Any(x => x.IsMultiLevel());
    }
    /// <summary>
    ///     Shortcut for doing Child[name] = Description.For(child)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="child"></param>
    internal void AddChild(string name, object child)
    {
        Children[name] = For(child);
    }
}