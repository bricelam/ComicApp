using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

static class ComStreamExtensions
{
    public static Stream AsStream(this IStream comStream)
        => new ComStream(comStream);

    class ComStream : Stream
    {
        readonly IStream _stream;
        long _position;

        public ComStream(IStream stream)
        {
            stream.Stat(out var statstg, STATFLAG.STATFLAG_NONAME);
            Length = (long)statstg.cbSize;

            _stream = stream;
        }

        public override bool CanRead
            => true;

        public override bool CanSeek
            => true;

        public override bool CanWrite
            => true;

        public override long Length { get; }

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            var hr = _stream.Read(buffer, out var bytesRead);
            _position += bytesRead;
            Marshal.ThrowExceptionForHR(hr);

            return (int)bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _stream.Seek(offset, origin, out var newPosition);
            _position = (long)newPosition;

            return _position;
        }

        public override void SetLength(long value)
            => _stream.SetSize((ulong)value);

        public override void Write(byte[] buffer, int offset, int count)
            => Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            var hr = _stream.Write(buffer, out var bytesWritten);
            _position += bytesWritten;
            Marshal.ThrowExceptionForHR(hr);
        }
    }
}
