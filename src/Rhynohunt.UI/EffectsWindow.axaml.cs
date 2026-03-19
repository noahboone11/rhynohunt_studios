using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Rhynohunt.Core;
using Rhynohunt.AudioEngine;

namespace Rhynohunt.UI;

public partial class EffectsWindow : Window
{
    public EffectsWindow(Track track)
    {
        InitializeComponent();
        DataContext = track;
    }
}