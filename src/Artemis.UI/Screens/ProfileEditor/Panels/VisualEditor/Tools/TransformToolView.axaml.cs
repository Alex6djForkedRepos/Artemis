using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Providers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.ReactiveUI;
using Avalonia.Skia;
using Avalonia.VisualTree;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class TransformToolView : ReactiveUserControl<TransformToolViewModel>
{
    private readonly List<Control> _handles = new();
    private readonly Panel _resizeBottomCenter;
    private readonly Panel _resizeBottomLeft;
    private readonly Panel _resizeBottomRight;
    private readonly Panel _resizeLeftCenter;
    private readonly Panel _resizeRightCenter;
    private readonly Panel _resizeTopCenter;
    private readonly Panel _resizeTopLeft;
    private readonly Panel _resizeTopRight;

    private SKPoint _dragOffset;
    private ZoomBorder? _zoomBorder;
    private readonly Grid _handleGrid;

    public TransformToolView()
    {
        InitializeComponent();

        _handleGrid = this.Get<Grid>("HandleGrid");

        _handles.Add(this.Get<Ellipse>("RotateTopLeft"));
        _handles.Add(this.Get<Ellipse>("RotateTopRight"));
        _handles.Add(this.Get<Ellipse>("RotateBottomRight"));
        _handles.Add(this.Get<Ellipse>("RotateBottomLeft"));

        _resizeTopCenter = this.Get<Panel>("ResizeTopCenter");
        _handles.Add(_resizeTopCenter);
        _resizeRightCenter = this.Get<Panel>("ResizeRightCenter");
        _handles.Add(_resizeRightCenter);
        _resizeBottomCenter = this.Get<Panel>("ResizeBottomCenter");
        _handles.Add(_resizeBottomCenter);
        _resizeLeftCenter = this.Get<Panel>("ResizeLeftCenter");
        _handles.Add(_resizeLeftCenter);
        _resizeTopLeft = this.Get<Panel>("ResizeTopLeft");
        _handles.Add(_resizeTopLeft);
        _resizeTopRight = this.Get<Panel>("ResizeTopRight");
        _handles.Add(_resizeTopRight);
        _resizeBottomRight = this.Get<Panel>("ResizeBottomRight");
        _handles.Add(_resizeBottomRight);
        _resizeBottomLeft = this.Get<Panel>("ResizeBottomLeft");
        _handles.Add(_resizeBottomLeft);

        _handles.Add(this.Get<Panel>("AnchorPoint"));

        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Rotation).Subscribe(_ => UpdateTransforms()).DisposeWith(d));
    }

    private void UpdateTransforms()
    {
        if (_zoomBorder == null || ViewModel == null)
            return;

        double resizeSize = Math.Clamp(1 / _zoomBorder.ZoomX, 0.2, 2);
        TransformOperations.Builder builder = TransformOperations.CreateBuilder(2);
        builder.AppendScale(resizeSize, resizeSize);

        TransformOperations counterScale = builder.Build();
        RotateTransform counterRotate = new(ViewModel.Rotation * -1);

        // Apply the counter rotation to the containers
        foreach (Panel panel in _handleGrid.Children.Where(c => c is Panel and not Canvas).Cast<Panel>())
            panel.RenderTransform = counterRotate;

        foreach (Control control in _handleGrid.GetVisualDescendants().Where(d => d is Control c && c.Classes.Contains("unscaled")).Cast<Control>())
            control.RenderTransform = counterScale;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private SKPoint GetPositionForViewModel(PointerEventArgs e)
    {
        if (ViewModel?.Layer == null)
            return SKPoint.Empty;

        SKPoint point = CounteractLayerRotation(e.GetCurrentPoint(this).Position.ToSKPoint(), ViewModel.Layer);
        return point + _dragOffset;
    }

    private static SKPoint CounteractLayerRotation(SKPoint point, Layer layer)
    {
        SKPoint pivot = layer.GetLayerAnchorPosition();

        using SKPath counterRotatePath = new();
        counterRotatePath.AddPoly(new[] {SKPoint.Empty, point}, false);
        counterRotatePath.Transform(SKMatrix.CreateRotationDegrees(layer.Transform.Rotation.CurrentValue * -1, pivot.X, pivot.Y));

        return counterRotatePath.Points[1];
    }

    private TransformToolViewModel.ResizeSide GetResizeDirection(Ellipse element)
    {
        if (ReferenceEquals(element.Parent, _resizeTopLeft))
            return TransformToolViewModel.ResizeSide.Top | TransformToolViewModel.ResizeSide.Left;
        if (ReferenceEquals(element.Parent, _resizeTopRight))
            return TransformToolViewModel.ResizeSide.Top | TransformToolViewModel.ResizeSide.Right;
        if (ReferenceEquals(element.Parent, _resizeBottomRight))
            return TransformToolViewModel.ResizeSide.Bottom | TransformToolViewModel.ResizeSide.Right;
        if (ReferenceEquals(element.Parent, _resizeBottomLeft))
            return TransformToolViewModel.ResizeSide.Bottom | TransformToolViewModel.ResizeSide.Left;
        if (ReferenceEquals(element.Parent, _resizeTopCenter))
            return TransformToolViewModel.ResizeSide.Top;
        if (ReferenceEquals(element.Parent, _resizeRightCenter))
            return TransformToolViewModel.ResizeSide.Right;
        if (ReferenceEquals(element.Parent, _resizeBottomCenter))
            return TransformToolViewModel.ResizeSide.Bottom;
        if (ReferenceEquals(element.Parent, _resizeLeftCenter))
            return TransformToolViewModel.ResizeSide.Left;

        throw new ArgumentException("Given element is not a child of a resize container");
    }

    #region Zoom

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _zoomBorder = (ZoomBorder?) this.GetLogicalAncestors().FirstOrDefault(l => l is ZoomBorder);
        if (_zoomBorder != null)
            _zoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_zoomBorder != null)
            _zoomBorder.PropertyChanged -= ZoomBorderOnPropertyChanged;
        base.OnDetachedFromLogicalTree(e);
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ZoomBorder.ZoomXProperty || _zoomBorder == null)
            return;

        UpdateTransforms();
    }

    #endregion

    #region Movement

    private void MoveOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        SKPoint dragStart = e.GetCurrentPoint(this).Position.ToSKPoint();
        SKRect shapeBounds = ViewModel.Layer.GetLayerPath(true, true, false).Bounds;
        _dragOffset = new SKPoint(dragStart.X - shapeBounds.Left, dragStart.Y - shapeBounds.Top);

        ViewModel.StartMovement();
        ToolTip.SetTip((Control) sender, $"X: {ViewModel.Layer.Transform.Position.CurrentValue.X:F3}% Y: {ViewModel.Layer.Transform.Position.CurrentValue.Y:F3}%");
        ToolTip.SetIsOpen((Control) sender, true);

        e.Pointer.Capture((IInputElement?) sender);
        e.Handled = true;
    }

    private void MoveOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer.Captured, sender) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        SKPoint position = e.GetCurrentPoint(this).Position.ToSKPoint() - _dragOffset;
        ViewModel.UpdateMovement(position, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
        ToolTip.SetTip((Control) sender, $"X: {ViewModel.Layer.Transform.Position.CurrentValue.X:F3}% Y: {ViewModel.Layer.Transform.Position.CurrentValue.Y:F3}%");

        e.Handled = true;
    }

    private void MoveOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender == null || !ReferenceEquals(e.Pointer.Captured, sender) || e.InitialPressMouseButton != MouseButton.Left || ViewModel?.Layer == null)
            return;

        ViewModel.FinishMovement();
        ToolTip.SetTip((Control) sender, null);
        ToolTip.SetIsOpen((Control) sender, false);
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    #endregion

    #region Anchor movement

    private void AnchorOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        SKPoint dragStart = e.GetCurrentPoint(this).Position.ToSKPoint();
        _dragOffset = dragStart - ViewModel.Layer.GetLayerAnchorPosition();

        ViewModel.StartAnchorMovement(e.GetCurrentPoint(this).Position.ToSKPoint() - _dragOffset);
        ToolTip.SetTip((Control) sender, $"X: {ViewModel.Layer.Transform.AnchorPoint.CurrentValue.X:F3}% Y: {ViewModel.Layer.Transform.AnchorPoint.CurrentValue.Y:F3}%");
        ToolTip.SetIsOpen((Control) sender, true);

        e.Pointer.Capture((IInputElement?) sender);
        e.Handled = true;
    }

    private void AnchorOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer.Captured, sender) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        SKPoint position = e.GetCurrentPoint(this).Position.ToSKPoint() - _dragOffset;
        ViewModel.UpdateAnchorMovement(position, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
        ToolTip.SetTip((Control) sender, $"X: {ViewModel.Layer.Transform.AnchorPoint.CurrentValue.X:F3}% Y: {ViewModel.Layer.Transform.AnchorPoint.CurrentValue.Y:F3}%");

        e.Handled = true;
    }

    private void AnchorOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender == null || !ReferenceEquals(e.Pointer.Captured, sender) || e.InitialPressMouseButton != MouseButton.Left || ViewModel?.Layer == null)
            return;

        ViewModel.FinishAnchorMovement();
        ToolTip.SetTip((Control) sender, null);
        ToolTip.SetIsOpen((Control) sender, false);

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    #endregion

    #region Resizing

    private void ResizeOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        SKPoint dragStart = CounteractLayerRotation(e.GetCurrentPoint(this).Position.ToSKPoint(), ViewModel.Layer);
        _dragOffset = ViewModel.Layer.GetDragOffset(dragStart);
        ToolTip.SetTip((Control) sender, $"Width: {ViewModel.Layer.Transform.Scale.CurrentValue.Width:F3}% Height: {ViewModel.Layer.Transform.Scale.CurrentValue.Height:F3}%");
        ToolTip.SetIsOpen((Control) sender, true);

        ViewModel.StartResize();

        e.Pointer.Capture((IInputElement?) sender);
        e.Handled = true;
    }

    private void ResizeOnPointerMoved(object? sender, PointerEventArgs e)
    {
        UpdateCursors();
        if (sender is not Ellipse element || !ReferenceEquals(e.Pointer.Captured, element) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        SKPoint position = GetPositionForViewModel(e);
        ViewModel.UpdateResize(
            position,
            GetResizeDirection(element),
            e.KeyModifiers.HasFlag(KeyModifiers.Alt),
            e.KeyModifiers.HasFlag(KeyModifiers.Shift),
            e.KeyModifiers.HasFlag(KeyModifiers.Control)
        );
        ToolTip.SetTip((Control) sender, $"Width: {ViewModel.Layer.Transform.Scale.CurrentValue.Width:F3}% Height: {ViewModel.Layer.Transform.Scale.CurrentValue.Height:F3}%");

        e.Handled = true;
    }

    private void ResizeOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender == null || !ReferenceEquals(e.Pointer.Captured, sender) || e.InitialPressMouseButton != MouseButton.Left || ViewModel?.Layer == null)
            return;

        ViewModel.FinishResize();
        ToolTip.SetTip((Control) sender, null);
        ToolTip.SetIsOpen((Control) sender, false);

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void UpdateCursors()
    {
        _resizeTopCenter.Cursor = GetCursorAtAngle(0f);
        _resizeTopRight.Cursor = GetCursorAtAngle(45f);
        _resizeRightCenter.Cursor = GetCursorAtAngle(90f);
        _resizeBottomRight.Cursor = GetCursorAtAngle(135f);
        _resizeBottomCenter.Cursor = GetCursorAtAngle(180f);
        _resizeBottomLeft.Cursor = GetCursorAtAngle(225f);
        _resizeLeftCenter.Cursor = GetCursorAtAngle(270f);
        _resizeTopLeft.Cursor = GetCursorAtAngle(315f);
    }

    private Cursor GetCursorAtAngle(float angle, bool includeLayerRotation = true)
    {
        if (includeLayerRotation && ViewModel?.Layer != null)
            angle = (angle + ViewModel.Layer.Transform.Rotation.CurrentValue) % 360;

        if (angle is > 330 and <= 360 or >= 0 and <= 30)
            return new Cursor(StandardCursorType.TopSide);
        if (angle is > 30 and <= 60)
            return new Cursor(StandardCursorType.TopRightCorner);
        if (angle is > 60 and <= 120)
            return new Cursor(StandardCursorType.RightSide);
        if (angle is > 120 and <= 150)
            return new Cursor(StandardCursorType.BottomRightCorner);
        if (angle is > 150 and <= 210)
            return new Cursor(StandardCursorType.BottomSide);
        if (angle is > 210 and <= 240)
            return new Cursor(StandardCursorType.BottomLeftCorner);
        if (angle is > 240 and <= 300)
            return new Cursor(StandardCursorType.LeftSide);
        if (angle is > 300 and <= 330)
            return new Cursor(StandardCursorType.TopLeftCorner);

        return Cursor.Default;
    }

    #endregion

    #region Rotation

    private float _rotationDragOffset;

    private void RotationOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        float startAngle = CalculateAngleToAnchor(e);
        _rotationDragOffset = startAngle - ViewModel.Layer.Transform.Rotation;
        ViewModel.StartRotation();
        ToolTip.SetTip((Control)sender, $"{ViewModel.Layer.Transform.Rotation.CurrentValue:F3}�");
        ToolTip.SetIsOpen((Control)sender, true);

        e.Pointer.Capture((IInputElement?) sender);
        e.Handled = true;
    }

    private void RotationOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender == null || !ReferenceEquals(e.Pointer.Captured, sender) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        float angle = CalculateAngleToAnchor(e);
        angle -= _rotationDragOffset;
        if (angle < 0)
            angle += 360;

        ViewModel?.UpdateRotation(angle, e.KeyModifiers.HasFlag(KeyModifiers.Control));
        ToolTip.SetTip((Control)sender, $"{ViewModel.Layer.Transform.Rotation.CurrentValue:F3}�");

        e.Handled = true;
    }

    private void RotationOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender == null || !ReferenceEquals(e.Pointer.Captured, sender) || e.InitialPressMouseButton != MouseButton.Left)
            return;

        ViewModel?.FinishRotation();
        ToolTip.SetTip((Control)sender, null);
        ToolTip.SetIsOpen((Control)sender, false);

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private float CalculateAngleToAnchor(PointerEventArgs e)
    {
        if (ViewModel?.Layer == null)
            return 0;

        SKPoint start = ViewModel.Layer.GetLayerAnchorPosition();
        SKPoint arrival = e.GetCurrentPoint(this).Position.ToSKPoint();

        float radian = (float) Math.Atan2(start.Y - arrival.Y, start.X - arrival.X);
        float angle = radian * (180f / (float) Math.PI);
        return angle;
    }

    #endregion
}