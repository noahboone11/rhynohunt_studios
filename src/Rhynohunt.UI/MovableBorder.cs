using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Rhynohunt.Core;

namespace Rhynohunt.UI;

public class MovableBorder : Border
{
    public static readonly StyledProperty<AudioClip?> AudioClipProperty =
        AvaloniaProperty.Register<MovableBorder, AudioClip?>(nameof(AudioClip));

    private bool _pressed;
    private readonly TranslateTransform _transform = new();
    private double _offsetX;

    public AudioClip? AudioClip
    {
        get => GetValue(AudioClipProperty);
        set => SetValue(AudioClipProperty, value);
    }

    public MovableBorder()
    {
        RenderTransform = _transform;

        AttachedToVisualTree += (_, _) =>
        {
            if (AudioClip != null)
                _transform.X = AudioClip.LeftPixels;
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AudioClipProperty && change.NewValue is AudioClip clip)
            _transform.X = clip.LeftPixels;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _pressed = true;

        var parent = Parent as Visual;
        if (parent == null) return;

        var currentPos = e.GetPosition(parent);
        _offsetX = currentPos.X - _transform.X;

        e.Pointer.Capture(this);
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_pressed) return;

        var parent = Parent as Visual;
        if (parent == null) return;

        var currentPos = e.GetPosition(parent);
        _transform.X = Math.Max(0, currentPos.X - _offsetX);

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _pressed = false;

        if (AudioClip != null)
            AudioClip.StartTime = TimeSpan.FromSeconds(_transform.X / 15.0);

        if (e.Pointer.Captured == this)
            e.Pointer.Capture(null);

        base.OnPointerReleased(e);
    }
}