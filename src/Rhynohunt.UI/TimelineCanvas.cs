using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Rhynohunt.AudioEngine;

namespace Rhynohunt.UI;

public class TimelineCanvas : Control
{
    public double PixelsPerSec { get; set; } = 15;

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<TimelineCanvas, IBrush?>(nameof(Background));

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly DirectProperty<TimelineCanvas, TransportController?> ControllerProperty =
        AvaloniaProperty.RegisterDirect<TimelineCanvas, TransportController?>(
            nameof(Controller),
            o => o.Controller,
            (o, v) => o.Controller = v);

    private TransportController? _controller;
    public TransportController? Controller
    {
        get => _controller;
        set => SetAndRaise(ControllerProperty, ref _controller, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_controller == null) return;

        var point = e.GetPosition(this);
        double seconds = Math.Max(0, point.X / PixelsPerSec);
        _controller.Seek(TimeSpan.FromSeconds(seconds));
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);
        double width = Bounds.Width;
        double height = Bounds.Height;
        var thinline = new Pen(Brushes.DimGray, 1);
        var thickline = new Pen(Brushes.DimGray, 2);
        var horizontalline = new Pen(Brushes.DimGray, 2.5);
        int totalsec = (int)(width / PixelsPerSec);

        drawingContext.DrawRectangle(Background, null, new Rect(0, 0, width, height));
        for (int second = 0; second < totalsec; second++)
        {
            var fmttext = new FormattedText($"{second}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 8, Brushes.DimGray);
            double time = second * PixelsPerSec;
            drawingContext.DrawText(fmttext, new Point(time - 1.5, 2));
            var pen = (second % 10 == 0) ? thickline : thinline;
            drawingContext.DrawLine(pen, new Point(time, 10), new Point(time, height));
        }

        int vert = 10;
        while (vert < height)
        {
            drawingContext.DrawLine(horizontalline, new Point(0, vert), new Point(width, vert));
            vert += 50;
        }
    }
}
