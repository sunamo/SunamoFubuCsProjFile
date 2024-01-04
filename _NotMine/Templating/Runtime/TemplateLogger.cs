namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;

public class TemplateLogger : ITemplateLogger
{
    private readonly Stopwatch _stopwatch = new();
    private int _alterationNumber;
    private int _indention;
    private int _numberOfAlterations;
    private int _numberOfSteps;
    private int _stepCount;

    public void Starting(int numberOfSteps)
    {
        _numberOfSteps = numberOfSteps;
        _stopwatch.Start();
    }

    public void TraceStep(ITemplateStep step)
    {
        _stepCount++;
        var text = _stepCount.ToString().PadLeft(3) + "/" + _numberOfSteps + ": " + step;

        ConsoleWriter.WriteWithIndent(ConsoleColor.White, 0, text);

        _indention = 4;
    }

    public void Trace(string contents, params string[] parameters)
    {
        ConsoleWriter.WriteWithIndent(ConsoleColor.White, _indention, contents.ToFormat(parameters));
    }

    public void StartProject(int numberOfAlterations)
    {
        _alterationNumber = 0;
        _numberOfAlterations = numberOfAlterations;
        _indention = 4;
    }

    public void EndProject()
    {
        _indention = 4;
    }

    public void TraceAlteration(string alteration)
    {
        _alterationNumber++;
        var text = _alterationNumber.ToString().PadLeft(3) + "/" + _numberOfAlterations + ": " + alteration;

        ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, _indention, text);
    }

    public void Finish()
    {
        _stopwatch.Stop();

        ConsoleWriter.Write(ConsoleColor.Green,
            "Templating successful in {0} ms".ToFormat(_stopwatch.ElapsedMilliseconds));
    }

    public void WriteSuccess(string message)
    {
        ConsoleWriter.WriteWithIndent(ConsoleColor.Green, _indention, message);
    }

    public void WriteWarning(string message)
    {
        ConsoleWriter.WriteWithIndent(ConsoleColor.Yellow, _indention, message);
    }
}
