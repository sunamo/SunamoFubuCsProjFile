namespace
#if SunamoGoogleSheets
SunamoGoogleSheets
#elif SunamoString
SunamoString
#elif SunamoStringShared
SunamoStringShared
#elif SunamoStringReplace
SunamoStringReplace
#else
SunamoFubuCsProjFile
#endif
;
public class None : Content
{
    public None()
    {
        Name = "None";
    }
    public None(string include) : base("None", include)
    {
    }
}