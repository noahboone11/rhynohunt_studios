using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Rhynohunt.AudioEngine;
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
        {
            Close(string.Empty);
        }
    }
}