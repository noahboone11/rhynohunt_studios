using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhynohunt.Core;
using System.Collections.ObjectModel;

public class Session : IDisposable
{
    // private readonly List<Track> _tracks = new List<Track>();
    private System.Timers.Timer? _autoSaveTimer;

    // public IReadOnlyList<Track> Tracks => _tracks;
    public ObservableCollection<Track> _tracks { get; } = new();

    public Track AddTrack(string name)
    {
        var track = new Track(name);
        _tracks.Add(track);
        return track;
    }

    public void RemoveTrack(Track track) => _tracks.Remove(track);

    public AudioClip LoadClipOnTrack(Track track, string filePath, TimeSpan startTime)
    {
        var clip = AudioClip.Load(filePath);
        track.AddClip(clip, startTime);
        return clip;
    }

    // Saves track layout to JSON — audio data is not stored, just file paths and positions
    public void Save(string filePath)
    {
        var data = new SessionData
        {
            Tracks = _tracks.Select(t => new TrackData
            {
                Name    = t.Name,
                Gain    = t.Gain,
                Pan     = t.Pan,
                IsMuted = t.IsMuted,
                IsSolo  = t.IsSolo,
                Clips   = t.Clips.Select(c => new ClipData
                {
                    FilePath         = c.FilePath,
                    StartTimeSeconds = c.StartTime.TotalSeconds
                }).ToList()
            }).ToList()
        };

        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static Session Load(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<SessionData>(json)
            ?? throw new InvalidDataException("Session file is empty or invalid.");

        var session = new Session();

        foreach (var td in data.Tracks)
        {
            var track = session.AddTrack(td.Name);
            track.Gain    = td.Gain;
            track.Pan     = td.Pan;
            track.IsMuted = td.IsMuted;
            track.IsSolo  = td.IsSolo;

            foreach (var cd in td.Clips)
            {
                var clip = AudioClip.Load(cd.FilePath);
                track.AddClip(clip, TimeSpan.FromSeconds(cd.StartTimeSeconds));
            }
        }

        return session;
    }

    public void EnableAutoSave(string filePath, int intervalMinutes)
    {
        DisableAutoSave();

        _autoSaveTimer = new System.Timers.Timer(intervalMinutes * 60_000);
        _autoSaveTimer.Elapsed  += (_, _) => Save(filePath);
        _autoSaveTimer.AutoReset = true;
        _autoSaveTimer.Start();
    }

    public void DisableAutoSave()
    {
        _autoSaveTimer?.Stop();
        _autoSaveTimer?.Dispose();
        _autoSaveTimer = null;
    }

    public void Dispose() => DisableAutoSave();

    // ── DTOs for JSON serialization ────────────────────────────────────────────

    private class SessionData
    {
        public List<TrackData> Tracks { get; set; } = new();
    }

    private class TrackData
    {
        public string Name    { get; set; } = "";
        public float  Gain    { get; set; } = 1.0f;
        public float  Pan     { get; set; } = 0.0f;
        public bool   IsMuted { get; set; }
        public bool   IsSolo  { get; set; }
        public List<ClipData> Clips { get; set; } = new();
    }

    private class ClipData
    {
        public string FilePath         { get; set; } = "";
        public double StartTimeSeconds { get; set; }
    }
}
