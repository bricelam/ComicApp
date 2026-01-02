using System.Formats.Tar;

namespace ComicLib.Archives;

class TarArchiveReader : ArchiveReader
{
    readonly Stream _stream;

    public static async Task<TarArchiveReader> CreateAsync(Stream stream, CancellationToken cancellationToken)
    {
        var result = new TarArchiveReader(stream);
        result._entryNames = await result.GetEntryNamesAsync(cancellationToken);

        return result;
    }

    public TarArchiveReader(Stream stream)
        => _stream = stream;

    string[]? _entryNames;
    public override IReadOnlyList<string> EntryNames
        => _entryNames ??= GetEntryNames();
    string[] GetEntryNames()
    {
        using var reader = OpenTar();

        return reader.GetEntries()
            .Select(e => e.Name)
            .Order()
            .ToArray();
    }
    async Task<string[]> GetEntryNamesAsync(CancellationToken cancellationToken)
    {
        await using var reader = OpenTar();

        return await reader.GetEntriesAsync(cancellationToken)
            .Select(e => e.Name)
            .Order()
            .ToArrayAsync(cancellationToken);
    }

    public override Stream? OpenEntry(string entryName)
    {
        using var reader = OpenTar();

        return reader.GetEntries()
            .FirstOrDefault(e => e.Name == entryName)
            ?.DataStream;
    }

    public override async Task<Stream?> OpenEntryAsync(string entryName, CancellationToken cancellationToken)
    {
        await using var reader = OpenTar();

        return (await reader.GetEntriesAsync(cancellationToken)
            .FirstOrDefaultAsync(e => e.Name == entryName, cancellationToken))
            ?.DataStream;
    }

    TarReader OpenTar()
    {
        _stream.Seek(0, SeekOrigin.Begin);

        return new(_stream, leaveOpen: true);
    }

#pragma warning disable CA2215
    public override void Dispose()
        => _stream.Dispose();

    public override ValueTask DisposeAsync()
        => _stream.DisposeAsync();
#pragma warning restore CA2215
}
