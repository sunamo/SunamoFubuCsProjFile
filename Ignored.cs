namespace SunamoFubuCsProjFile;

public class Ignored
{
    public class StartWith
    {
        public const string uap = @"\_Uap\";
        public const string archived = @"_Archived";
        public const string mixin = @"_Mixin";
    }

    public class EndsWith
    {
        public const string vcxProj = ".vcxproj";
    }

    public static bool IsIgnored(string path)
    {
        if (path == null) return true;
        return path.Contains(StartWith.archived) || path.Contains(StartWith.uap) || path.Contains(StartWith.mixin) ||
               path.EndsWith(EndsWith.vcxProj);
    }
}
