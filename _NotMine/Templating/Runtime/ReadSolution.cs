namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class ReadSolution : ITemplateStep
{
    public ReadSolution(string solutionFile)
    {
        SolutionFile = solutionFile;
    }

    public string SolutionFile { get; }

    public
        void
        Alter(TemplatePlan plan)
    {
        var solution =
                Solution.LoadFrom(SolutionFile)
#if ASYNC
                    .Result
#endif
            ;
        plan.Solution = solution;
    }

    protected bool Equals(ReadSolution other)
    {
        return string.Equals(SolutionFile, other.SolutionFile);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ReadSolution)obj);
    }

    public override int GetHashCode()
    {
        return SolutionFile != null ? SolutionFile.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return string.Format("Read solution {0}", SolutionFile);
    }
}
