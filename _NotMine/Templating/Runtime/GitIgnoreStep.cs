namespace SunamoFubuCsProjFile._NotMine.Templating.Runtime;



public class GitIgnoreStep : ITemplateStep
{
    public static readonly string File = "ignore.txt";

    public GitIgnoreStep(params string[] entries)
    {
        Entries = entries;
    }

    public string[] Entries { get; }

    public void Alter(TemplatePlan plan)
    {
        plan.AlterFile(".gitignore", list => Entries.Each(list.Fill));
    }


    /// <summary>
    /// Zatím bude sync, musím celý csproj více pochopit, vč. toho jak psát async
    /// Kromě toho bych měl zjistit zda FubuCsProj nemá nové zdrojáky. Nebo to nahradit jinou knihovnou.
    /// </summary>
    /// <param name="textFile"></param>
    /// <param name="plan"></param>
    public static
        //#if ASYNC
        //    async Task
        //#else
        //    void
        //#endif
        void
        ConfigurePlan(TextFile textFile, TemplatePlan plan)
    {
        var ignores =
            //#if ASYNC
            //    await
            //#endif
            textFile.ReadLines()
#if ASYNC
                .Result
#endif
                .Where(x => x.IsNotEmpty()).ToArray();
        var step = new GitIgnoreStep(ignores);
        plan.Add(step);
    }

    public override string ToString()
    {
        return string.Format("Adding to .gitignore: {0}", Entries.Join(", "));
    }
}
