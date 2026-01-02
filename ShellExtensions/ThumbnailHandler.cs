using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ComicLib;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.PropertiesSystem;

namespace ShellExtensions;

[ComVisible(true)]
[Guid("01BEFF85-52E0-410D-ABEE-1F004DA49064")]
public class ThumbnailHandler : IThumbnailProvider, IInitializeWithStream, IDisposable
{
    ComicBookArchive? _comicBook;

    public void Initialize(IStream pstream, uint grfMode)
    {
        // TODO: Free statstg.pwcsName?
        // TODO: Return ERROR_ALREADY_INITIALIZED?
        pstream.Stat(out var statstg, STATFLAG.STATFLAG_DEFAULT);

        _comicBook = new ComicBookArchive(pstream.AsStream(), statstg.pwcsName.ToString());
    }

    public unsafe void GetThumbnail(uint cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha)
    {
        using var coverStream = _comicBook!.CoverPage.Open();

        using var cover = Image.FromStream(coverStream);
        var scale = cx / (float)Math.Max(cover.Width, cover.Height);
        var width = (int)(cover.Width * scale);
        var height = (int)(cover.Height * scale);

        using var thumbnail = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(thumbnail))
        {
            graphics.DrawImage(cover, 0, 0, width, height);
        }

        *phbmp = new HBITMAP(thumbnail.GetHbitmap());
        *pdwAlpha = WTS_ALPHATYPE.WTSAT_ARGB;
    }

    // TODO: I don't think this gets called. Dispose in GetThumbnail?
    public void Dispose()
        => _comicBook?.Dispose();
}
