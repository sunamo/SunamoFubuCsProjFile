namespace SunamoFubuCsProjFile._NotMine.Templating.Planning;



/// <summary>
/// ITemplatePlannerActionAsync - Tohle budu muset více analyzovat a více se naučit async, abych věděl jak na to. Do té doby, commented.
/// </summary>
public abstract class TemplatePlanner : ITemplatePlannerAction /*, ITemplatePlannerActionAsync*/
{
    private readonly IList<ITemplatePlanner> _planners = new List<ITemplatePlanner>();
    private FileSet _matching;

    protected TemplatePlanner()
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
        ShallowMatch(GemReference.File).Do = GemReference.ConfigurePlan;
        ShallowMatch(GitIgnoreStep.File).Do = GitIgnoreStep.ConfigurePlan;

        ShallowMatch(RakeFileTransform.SourceFile).Do =
#if ASYNC
            async
#endif
                (file, plan) =>
            {
                plan.Add(new RakeFileTransform(
#if ASYNC
                    await
#endif
                        file.ReadAll()
                ));
            };
    }

    public Action<TextFile, TemplatePlan> Do
    {
        set
        {
            var planner = new FilesTemplatePlanner(_matching, value);
            _planners.Add(planner);
        }
    }

    // Tohle budu muset více analyzovat a více se naučit async, abych věděl jak na to.
    //Func<TextFile, TemplatePlan, Task> ITemplatePlannerActionAsync.Do { set => throw new NotImplementedException(); }

    //public ITemplatePlannerActionAsync MatchingAsync(FileSet matching)
    //{
    //    _matching = matching;
    //    return this;
    //}


    public void CreatePlan(Template template, TemplatePlan plan)
    {
        configurePlan(template.Path, plan);

        _planners.Each(x => x.DetermineSteps(template.Path, plan));

        plan.CopyUnhandledFiles(template.Path);
    }

    protected abstract void configurePlan(string directory, TemplatePlan plan);

    public void Add<T>() where T : ITemplatePlanner, new()
    {
        _planners.Add(new T());
    }

    public ITemplatePlannerAction Matching(FileSet matching)
    {
        _matching = matching;
        return this;
    }


    public ITemplatePlannerAction DeepMatch(string pattern)
    {
        return Matching(FileSet.Deep(pattern));
    }

    public ITemplatePlannerAction ShallowMatch(string pattern)
    {
        return Matching(FileSet.Shallow(pattern));
    }

    public ITemplatePlannerAction /*ITemplatePlannerActionAsync*/ ShallowMatchAsync(string pattern)
    {
        return Matching(FileSet.Shallow(pattern));
    }
}
