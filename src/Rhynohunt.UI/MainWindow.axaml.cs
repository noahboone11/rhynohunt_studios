using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Rhynohunt.Core;
using Rhynohunt.AudioEngine;
using Rhynohunt.Core;

namespace Rhynohunt.UI;

public partial class MainWindow : Window
{ 
    //Session
    //Controller
    public Session SESSION = new Session();
    TransportController controller = new TransportController(AudioEngine.AudioEngine.DefaultOutputDevice());
    private TranslateTransform playheadTransform = new();
    
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = SESSION;
    }

    private async void ImportClicked(Object sender, RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        
        var files = await toplevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose AudioClip",
            AllowMultiple = false
        });
        if (files.Count == 0)
        {
            return;
        }
        if (files.Count > 0)
        {
            Console.WriteLine("Here");
            var dialog = new NameDialog();

            var TrackName = await dialog.ShowDialog<string?>(this);
            Console.WriteLine($"Importing {TrackName}");
            var path = files[0].Path.LocalPath;
            var curtrack = SESSION.AddTrack(TrackName);
            SESSION.LoadClipOnTrack(curtrack,path,TimeSpan.FromSeconds(0));
            Console.WriteLine($"Imported {TrackName}");
        }
    }



    private async void playclicked(Object sender, RoutedEventArgs e)
    {
        if (controller.IsPlaying)
        {
            return;
        }

        foreach (var LoadedTracks in SESSION._tracks)
            {
                if ( !controller.Mixer.Tracks.Contains(LoadedTracks))
                {
                    controller.Mixer.AddTrack(LoadedTracks);
                }
            }
        

        controller.Play();
        
        PlayPosition.RenderTransform = playheadTransform;
        while (controller.IsPlaying)
        {
            playheadTransform.X = controller.CurrentTime.TotalSeconds * 15;
            await Task.Delay(16);
        }
        
    }
    private async void ExportClicked(object sender, RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);

        var folders = await toplevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Choose Export Location",
                AllowMultiple = false
            });

        if (folders.Count == 0)
            return;

        var folder = folders[0];
        var path = folder.TryGetLocalPath();

        if (string.IsNullOrEmpty(path))
            return;

        Core.AudioExporter.ExportWav(SESSION, path);
    }
    
    private void ClipRightClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not MovableBorder border || border.AudioClip == null)
            return;

        var clip = border.AudioClip;

        foreach (var track in SESSION._tracks)
        {
            if (track.Clips.Contains(clip))
            {
                DeleteClipFromTrack(track, clip);
                break;
            }
        }
    }

    private void stopclicked(Object sender, RoutedEventArgs e)
    {
        if (controller.IsPlaying)
        {            
            controller.Stop();
        }
        
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

    private void Pauseclicked(Object sender, RoutedEventArgs e)
    {
        controller.Pause();
    }

    private async void LoadClicked(object? sender, RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        var files = await toplevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose AudioClip",
            AllowMultiple = false
        });
        var path = files[0].Path.AbsolutePath;
        SESSION = Session.Load(path);
        
        DataContext = null;
        DataContext = SESSION;
    }

    private async void EffectsSelected(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Track track)
        {
            return;
        }
        var effectsDialog = new EffectsWindow(track);
        await effectsDialog.ShowDialog(this);
        Console.WriteLine(track.Gain);
    }

    private async void SaveClicked(object? sender, RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        var files = await toplevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose AudioClip",
            AllowMultiple = false
        });
        var path = files[0].Path.AbsolutePath;
        SESSION.Save(path);
    }
    private void DeleteClipFromTrack(Track track, AudioClip clip)
    {
        track.RemoveClip(clip);
        if (!track.HasClips)
        {
            SESSION.RemoveTrack(track);
        }
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
