namespace SunamoFubuCsProjFile._NotMine.Templating.Planning;

[Serializable]
public class MissingInputException : Exception
{
    public MissingInputException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public MissingInputException(IEnumerable<string> inputNames) : base(
        "Required inputs {0} are missing".ToFormat(inputNames.Join(", ")))
    {
        InputNames = inputNames;
    }

    public IEnumerable<string> InputNames { get; }
}
