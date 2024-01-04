namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class GemReference : ITemplateStep
{
    public static readonly string DefaultFeed = "source 'http://rubygems.org'";
    public static readonly string File = "gems.txt";

    public GemReference()
    {
    }

    public GemReference(string gemName, string version)
    {
        GemName = gemName;
        Version = version;
    }

    public string GemName { get; set; }
    public string Version { get; set; }

    public void Alter(TemplatePlan plan)
    {
        plan.AlterFile("Gemfile", Alter);
    }

    private
        void
        Alter(List<string> list)
    {
        if (!list.Contains(DefaultFeed))
        {
            list.Insert(0, DefaultFeed);
            list.Insert(1, string.Empty);
        }

        var key = "\"{0}\"".ToFormat(GemName);
        if (!list.Any(x => x.Contains(key)))
        {
            var line = "gem {0}, \"{1}\"".ToFormat(key, Version);
            list.Add(line);
        }
    }

    protected bool Equals(GemReference other)
    {
        return string.Equals(GemName, other.GemName) && string.Equals(Version, other.Version);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GemReference)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (GemName != null ? GemName.GetHashCode() : 0) * 397 ^
                   (Version != null ? Version.GetHashCode() : 0);
        }
    }

    public override string ToString()
    {
        return string.Format("Add gem to Gemfile: {0}, {1}", GemName, Version);
    }

    /// <summary>
    /// Zatím bude sync, musím celý csproj více pochopit, vč. toho jak psát async
    /// </summary>
    /// <param name="textFile"></param>
    /// <param name="plan"></param>
    public static
        //#if ASYNC
        //    async Task
        //#else
        //    void
        //#endif
        void
        ConfigurePlan(TextFile textFile, TemplatePlan plan)
    {
        //#if ASYNC
        //            await
        //#endif
        textFile.ReadLines()
#if ASYNC
            .Result
#endif
            .Each(line =>
            {
                var parts = line.ToDelimitedArray();
                plan.Add(new GemReference(parts.First(), parts.Last()));
            });
    }
}
