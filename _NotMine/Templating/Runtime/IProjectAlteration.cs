namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public interface IProjectAlteration
{
    /// <summary>
    /// must be async
    /// </summary>
    /// <param name="file"></param>
    /// <param name="plan"></param>
    /// <returns></returns>
#if ASYNC
    Task
#else
void
#endif
        Alter(CsprojFile file, ProjectPlan plan);
}
