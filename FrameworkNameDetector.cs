namespace SunamoFubuCsProjFile;



public class FrameworkNameDetector
{
    public const string DefaultIdentifier = ".NETFramework";
    public const string DefaultFrameworkVersion = "v4.0";

    /// <summary>
    ///     Working only for .net fw, for sdk style return .net 4.0
    ///     Must return FrameworkName due to FubuCsProjFile
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static FrameworkName Detect(MSBuildProject project)
    {
        var group = project.PropertyGroups.FirstOrDefault(x =>
            x.Properties.Any(p => p.Name.Contains("TargetFramework")));
        var identifier = DefaultIdentifier;
        var versionString = DefaultFrameworkVersion;
        string profile = null;

        if (group != null)
        {
            // .NETFramework
            identifier = group.GetPropertyValue("TargetFrameworkIdentifier") ?? DefaultIdentifier;
            // 4.8
            versionString = group.GetPropertyValue("TargetFrameworkVersion") ?? DefaultFrameworkVersion;
            // SE
            profile = group.GetPropertyValue("TargetFrameworkProfile");

            var version = Version.Parse(versionString.Replace("v", "").Replace("V", ""));

            Debug.WriteLine(identifier);
            Debug.WriteLine(version.ToString());
            Debug.WriteLine(profile);

            return new FrameworkName(identifier, version, profile);
        }

        return null; // DetectNetSdkVersion();
    }

    public static
#if ASYNC
        async Task<Version>
#else
Version
#endif
        Detect(string path)
    {
        var msb = new MSBuildProject();
#if ASYNC
        await msb.LoadAsync(path);
#else
msb.Load(path);
#endif

        return Detect(msb)?.Version;
    }
}
