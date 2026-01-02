using System.IO.Compression;

namespace ComicLib.Archives;

class ZipArchiveReader : ArchiveReader
{
    readonly ZipArchive _archive;

    public static async Task<ZipArchiveReader> CreateAsync(Stream stream, CancellationToken cancellationToken)
        => new ZipArchiveReader(
            await ZipArchive.CreateAsync(
                stream,
                ZipArchiveMode.Read,
                leaveOpen: false,
                entryNameEncoding: null,
                cancellationToken));

    public ZipArchiveReader(Stream stream)
        : this(new ZipArchive(stream))
    {
    }

    protected ZipArchiveReader(ZipArchive archive)
        => _archive = archive;

    public override string? Comment
        => _archive.Comment;

    string[]? _entryNames;
    public override IReadOnlyList<string> EntryNames
        => _entryNames ??= GetEntryNames();
    string[] GetEntryNames()
        => _archive.Entries
            .Select(e => e.FullName)
            .Order()
            .ToArray();

    public override Stream? OpenEntry(string entryName)
        => _archive.GetEntry(entryName)?.Open();

#pragma warning disable CS8619
    public override Task<Stream?> OpenEntryAsync(string entryName, CancellationToken cancellationToken)
        => _archive.GetEntry(entryName)?.OpenAsync(cancellationToken)
            ?? Task.FromResult<Stream?>(null);
#pragma warning restore CS8619

#pragma warning disable CA2215
    public override void Dispose()
        => _archive.Dispose();

    public override ValueTask DisposeAsync()
        => _archive.DisposeAsync();
#pragma warning restore CA2215
}
