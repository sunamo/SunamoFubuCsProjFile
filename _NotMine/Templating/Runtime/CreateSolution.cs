namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class CreateSolution : ITemplateStep
{
    public CreateSolution(string solutionName)
    {
        SolutionName = solutionName;
        Version = Solution.VS2012;
    }

    public string SolutionName { get; }

    public string Version { get; set; }

    /// <summary>
    /// must be async due to RakeFileTransform where is await
    /// </summary>
    /// <param name="plan"></param>
    /// <returns></returns>
    public
        void
        Alter(TemplatePlan plan)
    {
        var solution = Solution.CreateNew(plan.SourceDirectory, SolutionName);
        solution.Version = Version;

        plan.Solution = solution;
    }

    public override string ToString()
    {
        return string.Format("Create solution '{0}'", SolutionName);
    }
}
