namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class CopyProjectReferences : ITemplateStep
{
    public CopyProjectReferences(string originalProject)
    {
        OriginalProject = originalProject;
    }

    public string OriginalProject { get; }

    /// <summary>
    /// must be async due to RakeFileTransform where is await
    /// </summary>
    /// <param name="plan"></param>
    /// <returns></returns>
    public
        void
        Alter(TemplatePlan plan)
    {
        var original = plan.Solution.FindProject(OriginalProject).Project;
        var originalPlan = plan.FindProjectPlan(OriginalProject);

        var testPlan = plan.CurrentProject;
        var testProject = plan.Solution.FindProject(testPlan.ProjectName).Project;

        copyNugetDeclarations(originalPlan, testPlan, original, testProject);

        findNugetsInOriginalRippleDeclarations(plan, testPlan);

        buildProjectReference(original, testProject);
    }

    private static void copyNugetDeclarations(ProjectPlan originalPlan, ProjectPlan testPlan, CsprojFile original,
        CsprojFile testProject)
    {
        originalPlan.NugetDeclarations.Each(x => testPlan.NugetDeclarations.Fill(x));
        original.All<AssemblyReference>()
            .Where(x => x.HintPath.IsEmpty())
            .Each(x => testProject.Add<AssemblyReference>(x.Include));
    }

    private void findNugetsInOriginalRippleDeclarations(TemplatePlan plan, ProjectPlan testPlan)
    {
        var configFile = OriginalProject.ParentDirectory().AppendPath("ripple.dependencies.config");
        plan.FileSystem.ReadTextFile(configFile, line =>
        {
            if (line.IsNotEmpty()) testPlan.NugetDeclarations.Fill(line);
        });
    }

    private static void buildProjectReference(CsprojFile original, CsprojFile testProject)
    {
        var relativePathToTheOriginal = original.FileName.PathRelativeTo(testProject.FileName);
        if (original.FileName.ParentDirectory().ParentDirectory() ==
            testProject.FileName.ParentDirectory().ParentDirectory())
            relativePathToTheOriginal = Path.Combine("..", Path.GetFileName(original.FileName.ParentDirectory()),
                Path.GetFileName(original.FileName));


        var reference = new ProjectReference(relativePathToTheOriginal)
        {
            ProjectGuid = original.ProjectGuid,
            ProjectName = original.ProjectName
        };

        testProject.Add(reference);
    }

    public override string ToString()
    {
        return string.Format("Copy all references from {0}", OriginalProject);
    }
}
