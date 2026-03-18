using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Rhynohunt.Core;

namespace Rhynohunt.UI;

public class MovableBorder: Border
{
    public static readonly StyledProperty<AudioClip?> AudioClipProperty = AvaloniaProperty.Register<MovableBorder, AudioClip?>(nameof(AudioClip));
    
    private bool pressed;
    private Point positionofblock;
    private readonly TranslateTransform transform = new TranslateTransform();
    private double offsetX;

    public AudioClip? AudioClip
    {
        get => GetValue(AudioClipProperty);
        set => SetValue(AudioClipProperty, value);
    }

    public MovableBorder()
    {
        RenderTransform = transform;
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        pressed = true;
        var parent = Parent as Visual;
        if (parent == null) return;
        var CurrentPos = e.GetPosition(parent);
        offsetX = CurrentPos.X - transform.X;
        e.Pointer.Capture(this);
        double starttime = GetstartTime();
        Console.WriteLine($"Start time: {starttime} seconds");
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!pressed) return;
        var parent = Parent as Visual;
        if (parent == null) return;

        var CurrentPos = e.GetPosition(parent);

        transform.X = Math.Max(0,CurrentPos.X - offsetX);
        if (AudioClip != null)
        {
            AudioClip.StartTime = TimeSpan.FromSeconds(GetstartTime());
        }
        
        base.OnPointerMoved(e);
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        pressed = false;
        if (e.Pointer.Captured == this)
            e.Pointer.Capture(null);
        base.OnPointerReleased(e);
    }

    protected double GetstartTime()
    {
        double startpos = transform.X;
        return startpos / 15;
    }
}