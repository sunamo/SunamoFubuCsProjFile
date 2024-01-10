namespace SunamoFubuCsProjFile._NotMine.Templating.Planning;



public interface ITemplatePlannerActionAsync
{
#if ASYNC
    Func<TextFile, TemplatePlan, Task> Do { set; }
#else
Action<TextFile, TemplatePlan> Do { set; }
#endif
}

public interface ITemplatePlannerAction
{
    Action<TextFile, TemplatePlan> Do { set; }
}
