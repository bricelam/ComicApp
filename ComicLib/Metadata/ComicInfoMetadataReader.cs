using System.Text;
using System.Xml.Serialization;
using ComicLib.Archives;
using ComicLib.Metadata.Serialization;

namespace ComicLib.Metadata;

class ComicInfoMetadataReader : MetadataReader
{
    static readonly XmlSerializer _serializer = new(typeof(ComicInfo));

    public ComicInfoMetadataReader(ArchiveReader archiveReader, MetadataReader? fallbackReader = null)
        : base(archiveReader, fallbackReader)
    {
    }

    public override string? GetTitle()
    {
        if (ComicInfo?.Series is not null)
        {
            var builder = new StringBuilder();

            builder.Append(ComicInfo.Series);

            if (ComicInfo.Number is not null)
            {
                builder
                    .Append(" #")
                    .Append(ComicInfo.Number);
            }

            if (ComicInfo.Year.HasValue)
            {
                builder
                    .Append(" (")
                    .Append(ComicInfo.Year.Value)
                    .Append(')');
            }

            return builder.ToString();
        }

        return base.GetTitle();
    }

    public override string? GetAuthor()
        => ComicInfo?.Writer
            ?? base.GetAuthor();

    public override string? GetPublisher()
        => ComicInfo?.Publisher
            ?? base.GetPublisher();

    public override bool? GetIsRightToLeft()
    {
        if (ComicInfo is not null
            && ComicInfo.Manga.HasValue)
        {
            if (ComicInfo.Manga.Value == Manga.No)
                return false;

            if (ComicInfo.Manga.Value == Manga.YesAndRightToLeft)
                return true;
        }

        return base.GetIsRightToLeft();
    }

    public override string? GetDefaultMask()
        => base.GetDefaultMask(); // TODO

    public override bool? IsCoverPage(string entryName, int index)
    {
        if (ComicInfo?.Pages is not null
            && index < ComicInfo.Pages.Length)
        {
            return ComicInfo.Pages[index].Type == ComicPageType.FrontCover;
        }

        return base.IsCoverPage(entryName, index);
    }

    ComicInfo? _comicInfo;
    protected ComicInfo? ComicInfo
        => _comicInfo ??= GetComicInfo();
    ComicInfo? GetComicInfo()
    {
        using var stream = ArchiveReader.OpenEntry("ComicInfo.xml");
        if (stream is null)
            return null; // TODO: Don't keep retrying when null

        return (ComicInfo?)_serializer.Deserialize(stream);
    }
}
