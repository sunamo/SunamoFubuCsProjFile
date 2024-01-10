namespace SunamoFubuCsProjFile._NotMine.MSBuild;



public class MSBuildProjectSettings
{
    /// <summary>
    ///     When saving the project file, do we order the nodes
    ///     in ascending order?
    /// </summary>
    public bool MaintainOriginalItemOrder { get; set; }

    /// <summary>
    ///     When calls are made to <see cref="CsprojFile.Save()" />, only
    ///     save the file if the project differs from the one on disk.
    /// </summary>
    public bool OnlySaveIfChanged { get; set; }

    public static MSBuildProjectSettings DefaultSettings =>
        new()
        {
            MaintainOriginalItemOrder = false
        };

    public static MSBuildProjectSettings MinimizeChanges =>
        new()
        {
            MaintainOriginalItemOrder = true,
            OnlySaveIfChanged = true
        };
}
