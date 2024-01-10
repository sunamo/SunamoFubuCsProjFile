namespace SunamoFubuCsProjFile._NotMine;



public class CodeFile : ProjectItem
{
    private const string LinkAtt = "Link";

    public CodeFile(string relativePath) : base(ItemGroupsConsts.Compile, relativePath)
    {
    }

    public CodeFile() : base(ItemGroupsConsts.Compile)
    {
    }

    public string Link { get; set; }

    public override MSBuildItem Configure(MSBuildItemGroup group)
    {
        var item = base.Configure(group);
        UpdateMetadata();

        return item;
    }

    public override void Read(MSBuildItem item)
    {
        base.Read(item);


        Link = item.HasMetadata(LinkAtt) ? item.GetMetadata(LinkAtt) : null;
    }

    public override void Save()
    {
        base.Save();
        UpdateMetadata();
    }

    private void UpdateMetadata()
    {
        if (Link.IsNotEmpty()) BuildItem.SetMetadata(LinkAtt, Link);
    }
}
