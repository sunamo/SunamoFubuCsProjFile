namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public interface ITemplateStep
{
    // TODO: zjistit kde všude kvůli tomu že Alter je void užívám *Sync nebo .Result. Udělat si analýzu a případně to udělat asynchronnís

    /// <summary>
    /// must be void. again, must be void!!! RakeFileTransform.Alter should use await but in 10 other occurences they don't
    /// A to to celé brutálně zesložiťuje
    /// </summary>
    /// <param name="plan"></param>
    void Alter(TemplatePlan plan);
}
