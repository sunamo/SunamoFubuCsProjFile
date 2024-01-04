namespace SunamoFubuCsProjFile._NotMine;

public class TargetFrameworkVersion : IEquatable<TargetFrameworkVersion>, IComparable,
    IComparable<TargetFrameworkVersion>
{
    private static Type type = typeof(TargetFrameworkVersion);
    private readonly Version version;

    /// <summary>
    ///     Represents a target framework version
    /// </summary>
    /// <param name="version">In the form v2.0, v3.5, v4.0, v4.5, v4.5.1 etc...</param>
    public TargetFrameworkVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version)) ThrowEx.IsNull("version");

        this.version = new Version(version.TrimStart('v'));
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        return CompareTo((TargetFrameworkVersion)obj);
    }

    public int CompareTo(TargetFrameworkVersion other)
    {
        if (other == null) return 1;

        return version.CompareTo(other.version);
    }

    public bool Equals(TargetFrameworkVersion other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(version, other.version);
    }

    public override string ToString()
    {
        return string.Format("v{0}", version);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TargetFrameworkVersion)obj);
    }


    public override int GetHashCode()
    {
        return version != null ? version.GetHashCode() : 0;
    }

    public static bool operator ==(TargetFrameworkVersion left, TargetFrameworkVersion right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TargetFrameworkVersion left, TargetFrameworkVersion right)
    {
        return !Equals(left, right);
    }

    public static bool operator <(TargetFrameworkVersion x, TargetFrameworkVersion y)
    {
        if (x == null && y == null) return false;

        if (x == null) return true;

        return x.CompareTo(y) < 0;
    }

    public static bool operator >(TargetFrameworkVersion x, TargetFrameworkVersion y)
    {
        if (x == null && y == null) return false;

        if (x == null) return false;

        return x.CompareTo(y) > 0;
    }

    public static implicit operator string(TargetFrameworkVersion value)
    {
        return value == null ? null : value.ToString();
    }

    public static implicit operator TargetFrameworkVersion(string value)
    {
        return new TargetFrameworkVersion(value);
    }
}
