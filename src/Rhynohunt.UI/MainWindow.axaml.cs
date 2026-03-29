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

public partial class MainWindow : Window
{ 
    //Session
    //Controller
    public Session SESSION = new Session();
    TransportController controller = new TransportController(AudioEngine.AudioEngine.DefaultOutputDevice());
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = SESSION;
    }

    private async void ImportClicked(Object sender, RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        if (toplevel?.StorageProvider is null)
            return;
        
        var files = await toplevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose AudioClip",
            AllowMultiple = false
        });
        if (files.Count == 0)
            return;

        var trackPath = GetLocalPath(files[0]);
        if (trackPath is null)
            return;

        var dialog = new NameDialog();
        var trackName = await dialog.ShowDialog<string?>(this);
        if (string.IsNullOrWhiteSpace(trackName))
            return;

        Console.WriteLine($"Importing {trackName}");
        var currentTrack = SESSION.AddTrack(trackName.Trim());
        SESSION.LoadClipOnTrack(currentTrack, trackPath, TimeSpan.Zero);
        Console.WriteLine($"Imported {trackName}");
    }

    private void playclicked(Object sender, RoutedEventArgs e)
    {
        foreach (var LoadedTracks in SESSION._tracks)
        {
            controller.Mixer.AddTrack(LoadedTracks);
        }
        controller.Play();
        
    }

    private void stopclicked(Object sender, RoutedEventArgs e)
    {
        controller.Stop();
    }

    private void Addclip(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Track track)
            return;
        if (track.Clips.Count == 0)
            return;
        var clip = track.Clips.Last();
        var CopiedClip = AudioClip.Load(clip.FilePath);
        var lastend = track.Clips.Max(curclip => curclip.StartTime + curclip.Duration);
        track.AddClip(CopiedClip, lastend);
        Console.WriteLine(track.Clips.Count);
        
    }

    private async void LoadClicked(object? sender, RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        if (toplevel?.StorageProvider is null)
            return;

        var files = await toplevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose AudioClip",
            AllowMultiple = false
        });
        if (files.Count == 0)
            return;

        var path = GetLocalPath(files[0]);
        if (path is null)
            return;

        SESSION = Session.Load(path);
        
        DataContext = null;
        DataContext = SESSION;
    }

    private async void SaveClicked(object? sender, RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        if (toplevel?.StorageProvider is null)
            return;

        var files = await toplevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose AudioClip",
            AllowMultiple = false
        });
        if (files.Count == 0)
            return;

        var path = GetLocalPath(files[0]);
        if (path is null)
            return;

        SESSION.Save(path);
    }

    private static string? GetLocalPath(IStorageItem item)
    {
        if (item.Path.IsFile)
            return item.Path.LocalPath;

        return null;
    }
}
