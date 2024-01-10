namespace SunamoFubuCsProjFile._NotMine;

public class SourceControlInformation
{
    public SourceControlInformation(string projectUniqueName, string projectName, string projectLocalPath)
    {
        ProjectUniqueName = projectUniqueName;
        ProjectName = projectName;
        ProjectLocalPath = projectLocalPath;
    }

    public string ProjectUniqueName { get; private set; }
    public string ProjectName { get; private set; }
    public string ProjectLocalPath { get; private set; }
}
