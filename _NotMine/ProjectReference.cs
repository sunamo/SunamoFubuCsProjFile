namespace SunamoFubuCsProjFile._NotMine;



public class ProjectReference : ProjectItem
{
    public ProjectReference() : base(ItemGroupsConsts.ProjectReference)
    {
    }

    public ProjectReference(string include)
        : base(ItemGroupsConsts.ProjectReference, include)
    {
    }

    public ProjectReference(CsprojFile targetProject, CsprojFile reference) : base(
        ItemGroupsConsts.ProjectReference)
    {
        Include = Path.Combine(reference.ProjectDirectory.PathRelativeTo(targetProject.ProjectDirectory),
            Path.GetFileName(reference.FileName));
        ProjectGuid = reference.ProjectGuid;
        ProjectName = reference.ProjectName;
    }

    public Guid ProjectGuid { get; set; }
    public string ProjectName { get; set; }

    /*
    *
    <ProjectReference Include="..\FubuCsprojFile\FubuCsprojFile.csproj">
    <Project>{5630FC3F-8C3E-4EAD-B960-B38FE3D87463}</Project>
    <Name>FubuCsprojFile</Name>
    </ProjectReference>
    *
    */

    public override MSBuildItem Configure(MSBuildItemGroup group)
    {
        var item = base.Configure(group);

        UpdateMetadata();

        return item;
    }

    public override void Read(MSBuildItem item)
    {
        base.Read(item);

        ProjectName = item.GetMetadata("Name");
        var raw = item.GetMetadata("Project").TrimStart('{').TrimEnd('}');
        ProjectGuid = Guid.Parse(raw);
    }

    public override void Save()
    {
        base.Save();
        UpdateMetadata();
    }

    private void UpdateMetadata()
    {
        BuildItem.SetMetadata("Project", "{{{0}}}".ToFormat(ProjectGuid).ToUpper());
        if (ProjectName != null) BuildItem.SetMetadata("Name", ProjectName);
    }
}
