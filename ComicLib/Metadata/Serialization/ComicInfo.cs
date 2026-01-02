using System.ComponentModel;
using System.Xml.Serialization;

namespace ComicLib.Metadata.Serialization;

[Serializable]
public class ComicInfo
{
    public string? Series { get; set; }
    public string? Number { get; set; }
    public int? Year { get; set; }
    public string? Writer { get; set; }
    public string? Publisher { get; set; }
    public Manga? Manga { get; set; }

    [XmlArrayItem("Page")]
    public ComicPageInfo[]? Pages { get; set; }
}

[Serializable]
public enum Manga
{
    Unknown,
    No,
    Yes,
    YesAndRightToLeft
}

[Serializable]
public class ComicPageInfo
{
    [XmlAttribute]
    public int Image { get; set; }

    [XmlAttribute]
    //[DefaultValue(ComicPageType.Story)]
    public ComicPageType Type { get; set; } = ComicPageType.Story;

    [XmlAttribute]
    public bool DoublePage { get; set; }

    //[XmlAttribute]
    //public string? Key { get; set; }
}

[Serializable]
public enum ComicPageType
{
    FrontCover,
    InnerCover,
    Roundup,
    Story,
    Advertisement,
    Editorial,
    Letters,
    Preview,
    BackCover,
    Other,
    Deleted
}