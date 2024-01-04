namespace SunamoFubuCsProjFile._NotMine.Templating.Planning;



public class SolutionPlanner : TemplatePlanner
{
    public SolutionPlanner()
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
        ShallowMatch(Substitutions.ConfigFile).Do = (file, plan) => { plan.Substitutions.ReadFrom(file.Path); };

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
                plan.Substitutions.ReadInputs(inputs, plan.MissingInputs.Add);
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
                plan.AddInstructions(instructions);
            };
    }

    protected override void configurePlan(string directory, TemplatePlan plan)
    {
        SolutionDirectory.PlanForDirectory(directory).Each(plan.Add);
    }
}
