namespace SunamoFubuCsProjFile._NotMine;



public class AssemblyReference : ProjectItem
{
    private const string HintPathAtt = "HintPath";

    public AssemblyReference() : base(ItemGroupsConsts.Reference)
    {
    }

    public AssemblyReference(string assemblyName) : base(ItemGroupsConsts.Reference, assemblyName)
    {
    }

    public AssemblyReference(string assemblyName, string hintPath) : this(assemblyName)
    {
        HintPath = hintPath;
    }

    public string HintPath { get; set; }
    public string FusionName { get; set; }
    public string DisplayName { get; set; }
    public bool? SpecificVersion { get; set; }
    public bool? Private { get; set; }
    public string Aliases { get; set; }

    public string AssemblyName
    {
        get
        {
            if (!string.IsNullOrEmpty(HintPath)) return Path.GetFileName(HintPath) ?? string.Empty;

            return string.Format("{0}.dll", Include.Split(',')[0]);
        }
    }

    public override MSBuildItem Configure(MSBuildItemGroup group)
    {
        var item = base.Configure(group);
        UpdateMetaData();

        return item;
    }

    public override void Save()
    {
        base.Save();
        UpdateMetaData();
    }

    public override void Read(MSBuildItem item)
    {
        base.Read(item);


        HintPath = item.HasMetadata(HintPathAtt) ? item.GetMetadata(HintPathAtt) : null;
        FusionName = item.HasMetadata("FusionName") ? item.GetMetadata("FusionName") : null;
        Aliases = item.HasMetadata("Aliases") ? item.GetMetadata("Aliases") : null;
        DisplayName = item.HasMetadata("Name") ? item.GetMetadata("Name") : null;

        if (item.HasMetadata("SpecificVersion")) SpecificVersion = bool.Parse(item.GetMetadata("SpecificVersion"));

        if (item.HasMetadata("Private")) Private = bool.Parse(item.GetMetadata("Private"));
    }

    private void UpdateMetaData()
    {
        if (HintPath.IsNotEmpty()) BuildItem.SetMetadata(HintPathAtt, HintPath);

        if (FusionName.IsNotEmpty()) BuildItem.SetMetadata("FusionName", FusionName);

        if (Aliases.IsNotEmpty()) BuildItem.SetMetadata("Aliases", Aliases);

        if (DisplayName.IsNotEmpty()) BuildItem.SetMetadata("Name", DisplayName);

        if (SpecificVersion.HasValue) BuildItem.SetMetadata("SpecificVersion", SpecificVersion.Value.ToString());

        if (Private.HasValue) BuildItem.SetMetadata("Private", Private.Value.ToString().ToLower());
    }
}
