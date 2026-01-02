using System.Runtime.CompilerServices;

#pragma warning disable IDE0130

namespace System.Formats.Tar;

static class TarReaderExtensions
{
    public static IEnumerable<TarEntry> GetEntries(this TarReader reader)
    {
        TarEntry? entry;
        while ((entry = reader.GetNextEntry()) is not null)
        {
            yield return entry;
        }
    }

    public static async IAsyncEnumerable<TarEntry> GetEntriesAsync(
        this TarReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        TarEntry? entry;
        while ((entry = await reader.GetNextEntryAsync(cancellationToken: cancellationToken)) is not null)
        {
            yield return entry;
        }
    }
}
