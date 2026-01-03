using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ComicLib;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System;

namespace ComicApp;

[INotifyPropertyChanged]
public sealed partial class MainWindow : Window
{
    double _dragHorizontalOffset;
    double _dragVerticalOffset;
    Point _dragStartPosition;

    [ObservableProperty]
    ComicBookArchive? _comicBook;

    [ObservableProperty]
    int _currentPage;

    [ObservableProperty]
    ImageSource? _imageSource;

    public MainWindow()
    {
        ComicBookArchive.SetSupportedImageExtensions(
            BitmapDecoder.GetDecoderInformationEnumerator()
                .SelectMany(i => i.FileExtensions)
                .ToArray());

        //_ = App.Current.GetRequiredService<>();

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(_titleBar);

        // TODO: Remember previous size and position
        var positionAndSize = GetDefaultPositionAndSize();
        AppWindow.MoveAndResize(positionAndSize);
    }

    RectInt32 GetDefaultPositionAndSize()
    {
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        var height = displayArea.WorkArea.Height;

        // TODO: Scroll viewer size should be 6.625 x 10.25
        var chromeHeight = AppWindow.Size.Height - AppWindow.ClientSize.Height;
        var chromeWidth = AppWindow.Size.Width - AppWindow.ClientSize.Width;

        var width = (int)((height - chromeHeight) * 6.625 / 10.25) + chromeWidth;
        var x = (displayArea.WorkArea.Width - width) / 2;

        return new(
            displayArea.WorkArea.X + x,
            displayArea.WorkArea.Y,
            width,
            height);
    }

    public async Task Open(StorageFile storageFile)
    {
        if (ComicBook is not null)
            await ComicBook.DisposeAsync();

        var stream = await storageFile.OpenReadAsync();

        ComicBook = await ComicBookArchive.CreateAsync(stream.AsStream(), storageFile.Path);
        CurrentPage = 0;

        UpdateImageSource();
    }

    void UpdateImageSource()
    {
        if (ComicBook is null)
            return;

        var bitmapImage = new BitmapImage();
        bitmapImage.SetSource(ComicBook.Pages[CurrentPage].Open().AsSeekable().AsRandomAccessStream());
        ImageSource = bitmapImage;
        _scrollViewer.ZoomToFactor(_scrollViewer.MinZoomFactor);
        UpdateMinZoomFactor();
    }

    // TODO: Arrow left
    private void HandlePrevious(object sender, RoutedEventArgs e)
    {
        if (ComicBook is null)
            return;

        CurrentPage--;

        if (CurrentPage < 0)
            CurrentPage = 0;

        UpdateImageSource();
    }

    // TODO: Arrow right
    void HandleNext(object sender, RoutedEventArgs e)
    {
        if (ComicBook is null)
            return;

        CurrentPage++;

        if (CurrentPage >= ComicBook.Pages.Count)
            CurrentPage = ComicBook.Pages.Count - 1;

        UpdateImageSource();
    }

    void HandleSizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateMinZoomFactor();

    // TODO: Handle images of different sizes
    void UpdateMinZoomFactor()
    {
        if (_image.ActualWidth == 0)
            return;

        var previousMinZoomFactor = _scrollViewer.MinZoomFactor;

        _scrollViewer.MinZoomFactor = (float)Math.Max(
            Math.Min(
                Math.Min(
                    _scrollViewer.ViewportWidth / _image.ActualWidth,
                    _scrollViewer.ViewportHeight / _image.ActualHeight),
                1.0),
            0.1);

        if (_scrollViewer.ZoomFactor == previousMinZoomFactor)
            _scrollViewer.ZoomToFactor(_scrollViewer.MinZoomFactor);
    }

    void HandlePointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(VirtualKeyModifiers.Shift))
        {
            _scrollViewer.ScrollToHorizontalOffset(
                _scrollViewer.HorizontalOffset - e.GetCurrentPoint(_scrollViewer).Properties.MouseWheelDelta);
            e.Handled = true;
        }
    }

    void HandlePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // TODO: And space pressed. Switch to hand cursor.
        var currentPoint = e.GetCurrentPoint(_scrollViewer);
        if (currentPoint.Properties.IsLeftButtonPressed)
        {
            _dragStartPosition = currentPoint.Position;
            _dragHorizontalOffset = _scrollViewer.HorizontalOffset;
            _dragVerticalOffset = _scrollViewer.VerticalOffset;
            _image.PointerMoved += HandlePointerMoved;
        }
    }

    void HandlePointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!e.GetCurrentPoint(_scrollViewer).Properties.IsLeftButtonPressed)
        {
            _image.PointerMoved -= HandlePointerMoved;

            return;
        }

        var currentPoint = e.GetCurrentPoint(_scrollViewer);
        _scrollViewer.ScrollToHorizontalOffset(_dragHorizontalOffset - (currentPoint.Position.X - _dragStartPosition.X));
        _scrollViewer.ScrollToVerticalOffset(_dragVerticalOffset - (currentPoint.Position.Y - _dragStartPosition.Y));
    }

    void HandleDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Link;
        e.DragUIOverride.IsGlyphVisible = false;
        e.DragUIOverride.Caption = "Open";
    }

    async void HandleDrop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            return;

        var storageItems = await e.DataView.GetStorageItemsAsync();
        var storageFile = (StorageFile)storageItems[0];
        await Open(storageFile);
    }
}
