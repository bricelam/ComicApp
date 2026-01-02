using ComicLib.Archives;

namespace ComicLib.Metadata;

class AcbfMetadataReader : MetadataReader
{
    public AcbfMetadataReader(ArchiveReader archiveReader, MetadataReader? fallbackReader = null)
        : base(archiveReader, fallbackReader)
    {
    }
}
