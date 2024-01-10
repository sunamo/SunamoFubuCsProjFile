namespace SunamoFubuCsProjFile._NotMine.MSBuild;



public class MSBuildProject
{
    private static XmlNamespaceManager manager;
    private readonly Dictionary<XmlElement, MSBuildObject> elemCache = new();
    private readonly string newLine = Environment.NewLine;
    private Dictionary<string, MSBuildItemGroup> bestGroups;
    public XmlDocument doc;

    public MSBuildProject()
    {
        Settings = MSBuildProjectSettings.DefaultSettings;
        doc = new XmlDocument();
        doc.PreserveWhitespace = false;
        doc.AppendChild(doc.CreateElement(null, "Project", Schema));
    }

    public static string Schema => Consts.Schema;

    public static XmlNamespaceManager XmlNamespaceManager
    {
        get
        {
            if (manager == null)
            {
                manager = new XmlNamespaceManager(new NameTable());
                manager.AddNamespace("tns", Schema);
            }

            return manager;
        }
    }

    public string DefaultTargets
    {
        get => doc.DocumentElement.GetAttribute("DefaultTargets");
        set => doc.DocumentElement.SetAttribute("DefaultTargets", value);
    }

    public Version ToolsVersion
    {
        get => new(doc.DocumentElement.GetAttribute("ToolsVersion"));
        set
        {
            if (value != null)
                doc.DocumentElement.SetAttribute("ToolsVersion", value.ToString());
            else
                doc.DocumentElement.RemoveAttribute("ToolsVersion");
        }
    }

    public FrameworkName FrameworkName => FrameworkNameDetector.Detect(this);

    public List<string> Imports
    {
        get
        {
            var ims = new List<string>();
            foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:Import", XmlNamespaceManager))
                ims.Add(elem.GetAttribute("Project"));
            return ims;
        }
    }

    public IEnumerable<MSBuildPropertyGroup> PropertyGroups
    {
        get
        {
            foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:PropertyGroup", XmlNamespaceManager))
                yield return GetGroup(elem);
        }
    }

    public IEnumerable<MSBuildItemGroup> ItemGroups
    {
        get
        {
            foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:ItemGroup", XmlNamespaceManager))
                yield return GetItemGroup(elem);
        }
    }

    public IEnumerable<MSBuildImport> ImportsItems
    {
        get
        {
            foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:Import", XmlNamespaceManager))
                yield return GetImport(elem);
        }
    }

    public MSBuildProjectSettings Settings { get; set; }

    public IEnumerable<MSBuildTarget> Targets
    {
        get
        {
            foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:Target", XmlNamespaceManager))
                yield return GetTarget(elem);
        }
    }

    public static MSBuildProject Create(string assemblyName)
    {
        var ass = Assembly.GetEntryAssembly(); //typeof(MSBuildProject).Assembly;

        // Pro zjištění jaké jsou dostupné resources.
        var rn = ass.GetManifestResourceNames();

        // v .net fw stačilo Project.txt, v .net core musí být FubuCsprojFile._NotMine.MSBuild.Project.txt


        var st = /*Assembly.GetExecutingAssembly()*/ ass.GetManifestResourceStream(typeof(MSBuildProject),
            "FubuCsprojFile._NotMine.MSBuild.Project.txt");

        var text = st.ReadAllText();

        return create(assemblyName, text);
    }

    private static MSBuildProject create(string assemblyName, string text)
    {
        text = text
            .Replace("FUBUPROJECTNAME", assemblyName);


        var project = new MSBuildProject();
        project.doc = new XmlDocument
        {
            PreserveWhitespace = false
        };

        project.doc.LoadXml(text);

        return project;
    }

    public static
#if ASYNC
        async Task<MSBuildProject>
#else
MSBuildProject
#endif
        CreateFromFile(string assemblyName, string file)
    {
        var text =
#if ASYNC
            await
#endif
                TextFile.FileSystem.ReadStringFromFile(file);
        return create(assemblyName, text);
    }

    public static MSBuildProject Parse(string assemblyName, string text)
    {
        return create(assemblyName, text);
    }

    public MSBuildItemGroup FindGroup(Func<MSBuildItem, bool> itemTest)
    {
        return ItemGroups.FirstOrDefault(x => x.Items.Any(itemTest));
    }

    public MSBuildImport FindImport(Func<MSBuildImport, bool> itemTest)
    {
        return ImportsItems.FirstOrDefault(itemTest);
    }

#if ASYNC
    public
#if ASYNC
        async Task<ResultWithException<XmlDocument>>
#else
ResultWithException<XmlDocument>
#endif
        LoadAsync(string file)
    {
        return
#if ASYNC
            await
#endif
                XmlDocumentsCache.Get(file);
    }
#else
public ResultWithException<XmlDocument> Load(string file)
{
return XmlDocumentsCache.Get(file);
}
#endif

    public void Save(string fileName)
    {
        // StringWriter.Encoding always returns UTF16. We need it to return UTF8, so the
        // XmlDocument will write the UTF8 header.

        if (!Settings.MaintainOriginalItemOrder)
            ItemGroups.Each(group =>
            {
                XmlElement[] elements = null;
                elements = group.Items.Select(x => x.Element)
                    .OrderBy(x => x.GetAttribute(ItemGroupAttrsConsts.Include)).ToArray();

                group.Element.RemoveAll();

                elements.Each(x => group.Element.AppendChild(x));
            });


        var sw = new ProjectWriter(ByteOrderMark.GetByName(Encodings.utf8));
        sw.NewLine = newLine;
        doc.Save(sw);

        var content = sw.ToString();
        if (!content.EndsWith(newLine))
            content += newLine;

        var shouldSave = !Settings.OnlySaveIfChanged ||
                         File.Exists(fileName) && !TFSE.ReadAllText(fileName).Equals(content);

        if (shouldSave)
            new FileSystem().WriteStringToFile(fileName, content);
    }

    public void AddNewImport(string name, string condition)
    {
        var elem = doc.CreateElement(null, "Import", Schema);
        elem.SetAttribute("Project", name);

        var last = doc.DocumentElement.SelectSingleNode("tns:Import[last()]", XmlNamespaceManager) as XmlElement;
        if (last != null)
            doc.DocumentElement.InsertAfter(elem, last);
        else
            doc.DocumentElement.AppendChild(elem);
    }

    public void RemoveImport(string name)
    {
        var elem =
            (XmlElement)
            doc.DocumentElement.SelectSingleNode("tns:Import[@Project='" + name + "']", XmlNamespaceManager);
        if (elem != null)
            elem.ParentNode.RemoveChild(elem);
        else
            //FIXME: should this actually log an error?
            CL.WriteLine("ppnf:");
    }

    public MSBuildPropertySet GetGlobalPropertyGroup()
    {
        var res = new MSBuildPropertyGroupMerged();
        foreach (var grp in PropertyGroups)
            if (grp.Condition.Length == 0)
                res.Add(grp);
        return res.GroupCount > 0 ? res : null;
    }

    /// <summary>
    ///     Gets the first debug property group matching the supplied <paramref name="platform" />.
    /// </summary>
    /// <param name="platform">defaults to the global default platform value</param>
    public MSBuildPropertyGroup GetDebugPropertyGroup(string platform = null)
    {
        if (platform == null) platform = GetGlobalPropertyGroup().GetPropertyValue("Platform");

        return PropertyGroups.First(item => item.Condition.Contains(string.Format("{0}|{1}", "Debug", platform),
            StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    ///     Gets the first release property group matching the supplied <paramref name="platform" />.
    /// </summary>
    /// <param name="platform">defaults to the global default platform value</param>
    public MSBuildPropertyGroup GetReleasePropertyGroup(string platform = null)
    {
        if (platform == null) platform = GetGlobalPropertyGroup().GetPropertyValue("Platform");

        return PropertyGroups.First(item => item.Condition.Contains(string.Format("{0}|{1}", "Release", platform),
            StringComparison.InvariantCultureIgnoreCase));
    }

    public MSBuildPropertyGroup AddNewPropertyGroup(bool insertAtEnd)
    {
        var elem = doc.CreateElement(null, "PropertyGroup", Schema);

        if (insertAtEnd)
        {
            var last =
                doc.DocumentElement.SelectSingleNode("tns:PropertyGroup[last()]",
                    XmlNamespaceManager) as XmlElement;
            if (last != null)
                doc.DocumentElement.InsertAfter(elem, last);
        }
        else
        {
            var first = doc.DocumentElement.SelectSingleNode("tns:PropertyGroup",
                XmlNamespaceManager) as XmlElement;
            if (first != null)
                doc.DocumentElement.InsertBefore(elem, first);
        }

        if (elem.ParentNode == null)
        {
            var first = doc.DocumentElement.SelectSingleNode("tns:ItemGroup", XmlNamespaceManager) as XmlElement;
            if (first != null)
                doc.DocumentElement.InsertBefore(elem, first);
            else
                doc.DocumentElement.AppendChild(elem);
        }

        return GetGroup(elem);
    }

    public MSBuildPropertyGroup AddNewPropertyGroup(MSBuildObject insertAfter)
    {
        var elem = doc.CreateElement(null, "PropertyGroup", Schema);
        doc.DocumentElement.InsertAfter(elem, insertAfter.Element);

        return GetGroup(elem);
    }

    public IEnumerable<MSBuildItem> GetAllItems()
    {
        foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:ItemGroup/*", XmlNamespaceManager))
            yield return GetItem(elem);
    }

    public IEnumerable<MSBuildItem> GetAllItems(params string[] names)
    {
        var name = string.Join("|tns:ItemGroup/tns:", names);
        foreach (
            XmlElement elem in doc.DocumentElement.SelectNodes("tns:ItemGroup/tns:" + name, XmlNamespaceManager))
            yield return GetItem(elem);
    }

    public MSBuildItemGroup AddNewItemGroup()
    {
        var elem = doc.CreateElement(null, "ItemGroup", Schema);
        doc.DocumentElement.AppendChild(elem);
        return GetItemGroup(elem);
    }

    public MSBuildItem AddNewItem(string name, string include)
    {
        var grp = FindBestGroupForItem(name);
        return grp.AddNewItem(name, include);
    }

    private MSBuildItemGroup FindBestGroupForItem(string itemName)
    {
        MSBuildItemGroup group;

        if (bestGroups == null)
        {
            bestGroups = new Dictionary<string, MSBuildItemGroup>();
        }
        else
        {
            if (bestGroups.TryGetValue(itemName, out group))
                return group;
        }

        foreach (var grp in ItemGroups)
            foreach (var it in grp.Items)
                if (it.Name == itemName)
                {
                    bestGroups[itemName] = grp;
                    return grp;
                }

        group = AddNewItemGroup();
        bestGroups[itemName] = group;
        return group;
    }

    public string GetProjectExtensions(string section)
    {
        var elem =
            doc.DocumentElement.SelectSingleNode("tns:ProjectExtensions/tns:" + section, XmlNamespaceManager) as
                XmlElement;
        if (elem != null)
            return elem.InnerXml;
        return string.Empty;
    }

    public void SetProjectExtensions(string section, string value)
    {
        var elem = doc.DocumentElement["ProjectExtensions", Schema];
        if (elem == null)
        {
            elem = doc.CreateElement(null, "ProjectExtensions", Schema);
            doc.DocumentElement.AppendChild(elem);
        }

        var sec = elem[section];
        if (sec == null)
        {
            sec = doc.CreateElement(null, section, Schema);
            elem.AppendChild(sec);
        }

        sec.InnerXml = value;
    }

    public void RemoveProjectExtensions(string section)
    {
        var elem =
            doc.DocumentElement.SelectSingleNode("tns:ProjectExtensions/tns:" + section, XmlNamespaceManager) as
                XmlElement;
        if (elem != null)
        {
            var parent = (XmlElement)elem.ParentNode;
            parent.RemoveChild(elem);
            if (!parent.HasChildNodes)
                parent.ParentNode.RemoveChild(parent);
        }
    }

    public void RemoveItem(MSBuildItem item)
    {
        elemCache.Remove(item.Element);
        var parent = (XmlElement)item.Element.ParentNode;
        item.Element.ParentNode.RemoveChild(item.Element);
        if (parent.ChildNodes.Count == 0)
        {
            elemCache.Remove(parent);
            parent.ParentNode.RemoveChild(parent);
            bestGroups = null;
        }
    }

    public MSBuildItem GetItem(XmlElement elem)
    {
        MSBuildObject ob;
        if (elemCache.TryGetValue(elem, out ob))
            return (MSBuildItem)ob;
        var it = new MSBuildItem(elem);
        elemCache[elem] = it;
        return it;
    }

    private MSBuildPropertyGroup GetGroup(XmlElement elem)
    {
        MSBuildObject ob;
        if (elemCache.TryGetValue(elem, out ob))
            return (MSBuildPropertyGroup)ob;
        var it = new MSBuildPropertyGroup(this, elem);
        elemCache[elem] = it;
        return it;
    }

    private MSBuildItemGroup GetItemGroup(XmlElement elem)
    {
        MSBuildObject ob;
        if (elemCache.TryGetValue(elem, out ob))
            return (MSBuildItemGroup)ob;
        var it = new MSBuildItemGroup(this, elem);
        elemCache[elem] = it;
        return it;
    }

    private MSBuildImport GetImport(XmlElement elem)
    {
        MSBuildObject ob;
        if (elemCache.TryGetValue(elem, out ob))
            return (MSBuildImport)ob;
        var it = new MSBuildImport(elem);
        elemCache[elem] = it;
        return it;
    }

    private MSBuildTarget GetTarget(XmlElement elem)
    {
        MSBuildObject ob;
        if (elemCache.TryGetValue(elem, out ob))
            return (MSBuildTarget)ob;
        var it = new MSBuildTarget(elem);
        elemCache[elem] = it;
        return it;
    }

    public void RemoveGroup(MSBuildPropertyGroup grp)
    {
        elemCache.Remove(grp.Element);
        grp.Element.ParentNode.RemoveChild(grp.Element);
    }

    public static
#if ASYNC
        async Task<MSBuildProject>
#else
MSBuildProject
#endif
        LoadFromAsync(string fileName)
    {
        var project = new MSBuildProject();

#if ASYNC
        await project.LoadAsync(fileName);
#else
project.Load(fileName);
#endif

        return project;
    }

    private class ProjectWriter : StringWriter
    {
        private readonly Encoding encoding;

        public ProjectWriter(ByteOrderMark bom)
        {
            encoding = bom != null ? Encoding.GetEncoding(ByteOrderMark.enum2String[bom.Name]) : null;
            ByteOrderMark = bom;
        }

        public ByteOrderMark ByteOrderMark { get; private set; }

        public override Encoding Encoding => encoding ?? Encoding.UTF8;
    }
}
