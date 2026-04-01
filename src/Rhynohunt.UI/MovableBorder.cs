using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Rhynohunt.Core;
using System.Threading.Tasks;


namespace Rhynohunt.UI;

public class MovableBorder : Border
{
    public static readonly StyledProperty<AudioClip?> AudioClipProperty =
        AvaloniaProperty.Register<MovableBorder, AudioClip?>(nameof(AudioClip));

    private bool pressed;
    private readonly TranslateTransform _transform = new();
    private double offsetX;
    public event Action<AudioClip>? HoveredClip; 
    
    public static readonly RoutedEvent<RoutedEventArgs> ClipRightClickedEvent = 
        RoutedEvent.Register<MovableBorder, RoutedEventArgs>(nameof(ClipRightClicked), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<RoutedEventArgs> HoveredClipEvent = 
        RoutedEvent.Register<MovableBorder, RoutedEventArgs>(nameof(HoveredClip), RoutingStrategies.Bubble);

    //Custom event trigger to register bar has been clicked
    public event EventHandler<RoutedEventArgs> ClipRightClicked
    {
        add => AddHandler(ClipRightClickedEvent, value);
        remove => RemoveHandler(ClipRightClickedEvent, value);
    }

    //Links Bar to audio clip
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

    //When bar is clicked register drag for left and register right click
    protected override async void OnPointerPressed(PointerPressedEventArgs e)
    {
        var click = e.GetCurrentPoint(this);

        if (click.Properties.IsRightButtonPressed)
        {
            RaiseEvent(new RoutedEventArgs(ClipRightClickedEvent));
            e.Handled = true;
            return;
        }

        if (click.Properties.IsLeftButtonPressed)
        {
            pressed = true;

            var parent = Parent as Visual;
            if (parent == null) return;

            var currentPos = e.GetPosition(parent);
            offsetX = currentPos.X - _transform.X;

            ToolTip.SetIsOpen(this, false);
            ToolTip.SetTip(this, $"{AudioClip?.StartTime}");
            ToolTip.SetPlacement(this, PlacementMode.Pointer);

            await Task.Delay(1);
            ToolTip.SetIsOpen(this, true);

            e.Pointer.Capture(this);
            e.Handled = true;
        }

        base.OnPointerPressed(e);
    }
    
    //Update clip start time based on drag position
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!pressed) return;

        var parent = Parent as Visual;
        if (parent == null) return;

        var currentPos = e.GetPosition(parent);
        _transform.X = Math.Max(0, currentPos.X - offsetX);

        if (AudioClip != null)
        {
            var time = TimeSpan.FromSeconds(_transform.X / 15.0);
            ToolTip.SetTip(this, $"{time:mm\\:ss\\.ff}");
        }

        base.OnPointerMoved(e);
    }

    //Register release of click
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        pressed = false;

        if (AudioClip != null)
            AudioClip.StartTime = TimeSpan.FromSeconds(_transform.X / 15.0);

        if (e.Pointer.Captured == this)
            e.Pointer.Capture(null);
        
        ToolTip.SetIsOpen(this, false);

        base.OnPointerReleased(e);
    }
}