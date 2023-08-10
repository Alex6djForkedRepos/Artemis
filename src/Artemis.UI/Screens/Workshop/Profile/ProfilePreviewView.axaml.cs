using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Profile;

public partial class ProfilePreviewView : ReactiveUserControl<ProfilePreviewViewModel>
{
    private bool _movedByUser;

    public ProfilePreviewView()
    {
        InitializeComponent();

        ZoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
        ZoomBorder.PointerMoved += ZoomBorderOnPointerMoved;
        ZoomBorder.PointerWheelChanged += ZoomBorderOnPointerWheelChanged;
        UpdateZoomBorderBackground();

        this.WhenAnyValue(v => v.Bounds).Where(_ => !_movedByUser).Subscribe(_ => AutoFit());
    }

    private void ZoomBorderOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _movedByUser = true;
    }

    private void ZoomBorderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(ZoomBorder).Properties.IsMiddleButtonPressed)
            _movedByUser = true;
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(ZoomBorder.Background))
            UpdateZoomBorderBackground();
    }

    private void UpdateZoomBorderBackground()
    {
        if (ZoomBorder.Background is VisualBrush visualBrush)
            visualBrush.DestinationRect = new RelativeRect(ZoomBorder.OffsetX * -1, ZoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
    }

    private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        UpdateZoomBorderBackground();
    }

    private void AutoFit()
    {
        if (ViewModel == null || !ViewModel.Devices.Any())
            return;

        double left = ViewModel.Devices.Select(d => d.Rectangle.Left).Min();
        double top = ViewModel.Devices.Select(d => d.Rectangle.Top).Min();
        double bottom = ViewModel.Devices.Select(d => d.Rectangle.Bottom).Max();
        double right = ViewModel.Devices.Select(d => d.Rectangle.Right).Max();

        // Add a 10 pixel margin around the rect
        Rect scriptRect = new(new Point(left - 10, top - 10), new Point(right + 10, bottom + 10));

        // The scale depends on the available space
        double scale = Math.Min(3, Math.Min(Bounds.Width / scriptRect.Width, Bounds.Height / scriptRect.Height));

        // Pan and zoom to make the script fit
        ZoomBorder.Zoom(scale, 0, 0, true);
        ZoomBorder.Pan(Bounds.Center.X - scriptRect.Center.X * scale, Bounds.Center.Y - scriptRect.Center.Y * scale, true);

        _movedByUser = false;
    }
}