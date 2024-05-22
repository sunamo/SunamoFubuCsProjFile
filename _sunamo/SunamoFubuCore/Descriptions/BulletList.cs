namespace SunamoFubuCsProjFile;


internal class BulletList
{
    internal BulletList()
    {
        Children = new List<Description>();
    }
    internal IList<Description> Children { get; }
    internal string Name { get; set; }
    internal string Label { get; set; }
    internal bool IsOrderDependent { get; set; }
    internal void AcceptVisitor(IDescriptionVisitor visitor)
    {
        visitor.StartList(this);
        Children.Each(x => x.AcceptVisitor(visitor));
        visitor.EndList();
    }
}