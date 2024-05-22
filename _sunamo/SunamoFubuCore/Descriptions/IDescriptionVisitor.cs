namespace SunamoFubuCsProjFile;


internal interface IDescriptionVisitor
{
    void Start(Description description);
    void StartList(BulletList list);
    void EndList();
    void End();
}