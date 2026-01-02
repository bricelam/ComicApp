using System.Runtime.InteropServices;
using ComicLib;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.System.Variant;
using Windows.Win32.UI.Shell.PropertiesSystem;

using static Windows.Win32.PInvoke;

namespace ShellExtensions;

[ComVisible(true)]
[Guid("325F6AC5-FA0F-4456-A818-407B0E1CF7AE")]
public class PropertyHandler : IPropertyStore, IPropertyStoreCapabilities, IInitializeWithStream, IDisposable
{
    static readonly PROPERTYKEY[] _properties =
    [
        // TODO
        // System.Company
        // System.ItemDate
        // System.Document.DateCreated
        // System.Document.PageCount
        // System.Media.AuthorUrl
        // System.Media.DateReleased
        // System.Media.EpisodeNumber
        // System.Media.SeasonNumber
        // System.Media.SeriesName
        // System.Media.Writer
        // System.Media.Year
        PKEY_Author,
        PKEY_Title,
        PKEY_Media_Publisher
    ];

    ComicBookArchive _comicBook = null!;

    public void Initialize(IStream pstream, uint grfMode)
    {
        // TODO: Free statstg.pwcsName?
        // TODO: Return ERROR_ALREADY_INITIALIZED?
        pstream.Stat(out var statstg, STATFLAG.STATFLAG_DEFAULT);

        _comicBook = new ComicBookArchive(pstream.AsStream(), statstg.pwcsName.ToString());
    }

    public void GetCount(out uint cProps)
        => cProps = (uint)_properties.Length;

    public unsafe void GetAt(uint iProp, PROPERTYKEY* pkey)
        => *pkey = _properties[iProp];

    public unsafe void GetValue(PROPERTYKEY* key, out PROPVARIANT pv)
    {
        if (Equals(*key, PKEY_Author))
        {
            pv = CreateProp(_comicBook.Author);
        }
        else if (Equals(*key, PKEY_Title))
        {
            pv = CreateProp(_comicBook.Title);
        }
        else if (Equals(*key, PKEY_Media_Publisher))
        {
            pv = CreateProp(_comicBook.Publisher);
        }
        else
        {
            pv = default;
        }

        static PROPVARIANT CreateProp(string? value)
        {
            fixed (char* pValue = value)
            {
                return new PROPVARIANT
                {
                    Anonymous =
                    {
                        Anonymous =
                        {
                            vt = VARENUM.VT_LPWSTR,
                            Anonymous =
                            {
                                pwszVal = pValue
                            }
                        }
                    }
                };
            }
        }
    }

    public unsafe HRESULT IsPropertyWritable(PROPERTYKEY* key)
        => HRESULT.S_FALSE;

    public unsafe void SetValue(PROPERTYKEY* key, in PROPVARIANT propvar)
        => throw new NotSupportedException();

    public void Commit()
        => throw new NotSupportedException();

    public void Dispose()
        => _comicBook?.Dispose();
}
