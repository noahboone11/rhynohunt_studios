using Avalonia.Controls;
using Avalonia.Input;
using Track = Rhynohunt.Core.Track;

namespace Rhynohunt.UI;

public partial class EffectsWindow : Window
{
    public EffectsWindow(Track track)
    {
        InitializeComponent();
        DataContext = track;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
            Close(string.Empty);
    }
}
