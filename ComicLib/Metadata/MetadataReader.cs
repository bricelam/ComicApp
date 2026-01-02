using ComicLib.Archives;

namespace ComicLib.Metadata;

abstract class MetadataReader
{
    protected MetadataReader(ArchiveReader archiveReader, MetadataReader? fallbackReader = null)
    {
        ArchiveReader = archiveReader;
        FallbackReader = fallbackReader;
    }

    protected ArchiveReader ArchiveReader { get; }
    protected MetadataReader? FallbackReader { get; }

    public virtual string? GetTitle()
        => FallbackReader?.GetTitle();

    public virtual string? GetAuthor()
        => FallbackReader?.GetAuthor();

    public virtual string? GetPublisher()
        => FallbackReader?.GetPublisher();

    public virtual bool? GetIsRightToLeft()
        => FallbackReader?.GetIsRightToLeft();

    public virtual string? GetDefaultMask()
        => FallbackReader?.GetDefaultMask();

    public virtual bool? IsCoverPage(string entryName, int index)
        => FallbackReader?.IsCoverPage(entryName, index);
}
