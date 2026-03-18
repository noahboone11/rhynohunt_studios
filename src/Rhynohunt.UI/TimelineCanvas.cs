using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Rhynohunt.UI;

public class TimelineCanvas:Control
{
    public double PixelsPerSec { get; set; } = 15;

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<TimelineCanvas, IBrush?>(nameof(Background));
    
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
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
            var pen = (second % 10 == 0)? thickline : thinline;
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