#pragma warning disable IDE0130

namespace System.IO;

static class StreamExtensions
{
    public static Stream AsSeekable(this Stream stream, bool leaveOpen = false)
    {
        if (stream.CanSeek)
            return stream;

        var seekable = new MemoryStream();

        stream.CopyTo(seekable);

        if (!leaveOpen)
            stream.Dispose();

        seekable.Seek(0L, SeekOrigin.Begin);

        return seekable;
    }
}
