namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class SystemReference : IProjectAlteration
{
    public const string SourceFile = "references.txt";

    public SystemReference(string assemblyName)
    {
        AssemblyName = assemblyName;
    }

    public string AssemblyName { get; }

    /// <summary>
    /// Must be async even if there is no await due to IProjectAlteration
    /// </summary>
    /// <param name="file"></param>
    /// <param name="plan"></param>
    /// <returns></returns>
    public
#if ASYNC
        async Task
#else
void
#endif
        Alter(CsprojFile file, ProjectPlan plan)
    {
        file.Add<AssemblyReference>(AssemblyName);
    }

    protected bool Equals(SystemReference other)
    {
        return string.Equals(AssemblyName, other.AssemblyName);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SystemReference)obj);
    }

    public override int GetHashCode()
    {
        return AssemblyName != null ? AssemblyName.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return string.Format("Add assembly reference to {0}", AssemblyName);
    }
}
