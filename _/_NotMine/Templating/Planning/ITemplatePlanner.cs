namespace FubuCsprojFile.Templating.Planning
{
    public interface ITemplatePlanner
    {
        void DetermineSteps(string directory, TemplatePlan plan);
    }
}