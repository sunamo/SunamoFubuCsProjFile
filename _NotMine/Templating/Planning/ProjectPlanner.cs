namespace SunamoFubuCsProjFile._NotMine.Templating.Planning;



public class ProjectPlanner : TemplatePlanner
{
    public static readonly string NugetFile = "nuget.txt";

    public ProjectPlanner()
    {
    }

    public
#if ASYNC
        async Task
#else
void
#endif
        Init()
    {
        ShallowMatch(Substitutions.ConfigFile).Do = (file, plan) =>
        {
            plan.CurrentProject.Substitutions.ReadFrom(file.Path);
        };

        ShallowMatch(Input.File).Do =
#if ASYNC
            async
#endif
                (file, plan) =>
            {
                var inputs =
#if ASYNC
                    await
#endif
                        Input.ReadFromFile(file.Path);
                plan.CurrentProject.Substitutions.ReadInputs(inputs, plan.MissingInputs.Add);
            };

        Matching(FileSet.Shallow(ProjectPlan.TemplateFile)).Do = (file, plan) =>
        {
            plan.CurrentProject.ProjectTemplateFile = file.Path;
        };

        Matching(FileSet.Shallow(NugetFile)).Do =
#if ASYNC
            async
#endif
                (file, plan) =>
            {
                (
#if ASYNC
                        await
#endif
                            file.ReadLines())
                    .Where(x => x.IsNotEmpty())
                    .Each(line => plan.CurrentProject.NugetDeclarations.Add(line.Trim()));
            };

        Matching(FileSet.Shallow(AssemblyInfoAlteration.SourceFile)).Do =
#if ASYNC
            async
#endif
                (file, plan) =>
            {
                var additions = (
#if ASYNC
                    await
#endif
                        file.ReadLines()).Where(x => x.IsNotEmpty()).ToArray();
                plan.CurrentProject.Add(new AssemblyInfoAlteration(additions));
            };

        Matching(FileSet.Shallow(SystemReference.SourceFile)).Do =
#if ASYNC
            async
#endif
                (file, plan) =>
            {
                (
#if ASYNC
                        await
#endif

                            file.ReadLines())
                    .Where(x => x.IsNotEmpty())
                    .Each(assem => plan.CurrentProject.Add(new SystemReference(assem)));
            };

        Matching(FileSet.Deep("*.cs")).Do = async (file, plan) =>
        {
            var template = new CodeFileTemplate(file.RelativePath,
#if ASYNC
                await
#endif
                    file.ReadAll());
            plan.CurrentProject.Add(template);
        };

        ShallowMatch(TemplatePlan.InstructionsFile).Do =
#if ASYNC
            async
#endif
                (file, plan) =>
            {
                var instructions =
#if ASYNC
                    await
#endif
                        file.ReadAll();
                plan.AddInstructions(plan.ApplySubstitutions(instructions));
            };
    }

    protected override void configurePlan(string directory, TemplatePlan plan)
    {
        var current = plan.Steps.OfType<ProjectPlan>().LastOrDefault();
        ProjectDirectory.PlanForDirectory(directory).Each(x => current.Add(x));
    }
}
