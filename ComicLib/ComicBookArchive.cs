using ComicLib.Archives;
using ComicLib.Metadata;

namespace ComicLib;

public sealed class ComicBookArchive : IDisposable, IAsyncDisposable
{
    readonly static HashSet<string> _supportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".webp"
        // ".heic", ".svg", ".tif", ".tiff"
    };

    readonly string _path;
    readonly ArchiveReader _archiveReader;
    readonly MetadataReader _metadataReader;

    public static void SetSupportedImageExtensions(IEnumerable<string> value)
    {
        _supportedImageExtensions.Clear();
        _supportedImageExtensions.AddRange(value);
    }

    public static ComicBookArchive Open(string path)
        => new(File.OpenRead(path), path);

    public static Task<ComicBookArchive> OpenAsync(string path)
        => CreateAsync(File.OpenRead(path), path);

    public ComicBookArchive(Stream stream, string path)
    {
        _path = path;

        var extension = Path.GetExtension(path);
        _archiveReader = extension switch
        {
            ".cbz" or
            ".zip" => new ZipArchiveReader(stream),
            ".cbt" or
            ".tar" => new TarArchiveReader(stream),
            _ => throw new NotSupportedException($"Unsupported file type: {extension}")
        };

        _metadataReader = new AcbfMetadataReader(
            _archiveReader,
            new ComicInfoMetadataReader(
                _archiveReader));
    }

    public static async Task<ComicBookArchive> CreateAsync(
        Stream stream,
        string path,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(path);
        ArchiveReader archiveReader = extension switch
        {
            ".cbz" or
            ".zip" => await ZipArchiveReader.CreateAsync(stream, cancellationToken),
            ".cbt" or
            ".tar" => await TarArchiveReader.CreateAsync(stream, cancellationToken),
            _ => throw new NotSupportedException($"Unsupported file type: {extension}")
        };
        var metadataReader = new AcbfMetadataReader(
            archiveReader,
            new ComicInfoMetadataReader(
                archiveReader));

        return new ComicBookArchive(path, archiveReader, metadataReader);
    }

    internal ComicBookArchive(string path, ArchiveReader archiveReader, MetadataReader metadataReader)
    {

        _path = path;
        _archiveReader = archiveReader;
        _metadataReader = metadataReader;
    }

    public string Title
        => _metadataReader.GetTitle()
            ?? Path.GetFileNameWithoutExtension(_path);

    public string? Author
        => _metadataReader.GetAuthor();

    public string? Publisher
        => _metadataReader.GetPublisher();

    public bool IsRightToLeft
        => _metadataReader.GetIsRightToLeft()
            ?? false;

    ComicBookPage? _coverPage;
    public ComicBookPage CoverPage
        => _coverPage ??= GetCoverPage();
    ComicBookPage GetCoverPage()
        => Pages.First(p => p.IsCoverPage);

    ComicBookPage[]? _pages;
    public IReadOnlyList<ComicBookPage> Pages
        => _pages ??= GetPages();
    ComicBookPage[] GetPages()
        => _archiveReader.EntryNames
            .Where(p => _supportedImageExtensions.Contains(Path.GetExtension(p)))
            .Select((p, i) => new ComicBookPage(_archiveReader, _metadataReader, p, i))
            .ToArray();

    public void Dispose()
        => _archiveReader.Dispose();

    public ValueTask DisposeAsync()
        => _archiveReader.DisposeAsync();
}
