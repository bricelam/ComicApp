
namespace ComicLib.Archives;

abstract class ArchiveReader : IDisposable, IAsyncDisposable
{
    public virtual string? Comment
        => null;

    public abstract IReadOnlyList<string> EntryNames { get; }

    public abstract Stream? OpenEntry(string entryName);

    public abstract Task<Stream?> OpenEntryAsync(string entryName, CancellationToken cancellationToken);

    public virtual void Dispose()
    {
    }

    public virtual ValueTask DisposeAsync()
        => ValueTask.CompletedTask;
}
