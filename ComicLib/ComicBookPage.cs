using ComicLib.Archives;
using ComicLib.Metadata;

namespace ComicLib;

public class ComicBookPage
{
    readonly ArchiveReader _archiveReader;
    readonly MetadataReader _metadataReader;
    readonly string _entryName;
    readonly int _index;

    internal ComicBookPage(ArchiveReader archiveReader, MetadataReader metadataReader, string entryName, int index)
    {
        _archiveReader = archiveReader;
        _metadataReader = metadataReader;
        _entryName = entryName;
        _index = index;
    }

    public bool IsCoverPage
        => _metadataReader.IsCoverPage(_entryName, _index)
            ?? _index == 0;

    // TODO
    // Number
    // ToC
    // IsFacingPage
    // Pannels
    //    Ordinal
    //    X,Y,Width,Height
    //    Mask (ARGB)

    public Stream Open()
        => _archiveReader.OpenEntry(_entryName)!;
}
