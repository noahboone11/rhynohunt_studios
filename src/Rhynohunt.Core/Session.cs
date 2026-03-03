using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhynohunt.Core;

/// <summary>
/// Represents a recording session — the top-level container for a collection of
/// <see cref="Track"/> objects. Supports adding and removing tracks, loading clips
/// onto tracks, JSON-based save/load, and timed auto-save.
/// Implements <see cref="IDisposable"/> to release the auto-save timer.
/// </summary>
public class Session : IDisposable
{
    private readonly List<Track> _tracks = new List<Track>();
    private System.Timers.Timer? _autoSaveTimer;

    /// <summary>Gets the read-only list of tracks in this session.</summary>
    public IReadOnlyList<Track> Tracks => _tracks;

    /// <summary>
    /// Creates a new <see cref="Track"/> with the given name, adds it to the session,
    /// and returns it for further configuration.
    /// </summary>
    /// <param name="name">The display name for the new track.</param>
    /// <returns>The newly created <see cref="Track"/>.</returns>
    public Track AddTrack(string name)
    {
        var track = new Track(name);
        _tracks.Add(track);
        return track;
    }

    /// <summary>Removes the specified track from the session.</summary>
    /// <param name="track">The track to remove.</param>
    public void RemoveTrack(Track track) => _tracks.Remove(track);

    /// <summary>
    /// Loads an <see cref="AudioClip"/> from the given file path and adds it to
    /// the specified track at the given timeline position.
    /// </summary>
    /// <param name="track">The track to add the clip to. Must belong to this session.</param>
    /// <param name="filePath">The absolute or relative path to the audio file (WAV or MP3).</param>
    /// <param name="startTime">The position on the timeline where the clip begins.</param>
    /// <returns>The loaded <see cref="AudioClip"/>.</returns>
    public AudioClip LoadClipOnTrack(Track track, string filePath, TimeSpan startTime)
    {
        var clip = AudioClip.Load(filePath);
        track.AddClip(clip, startTime);
        return clip;
    }

    /// <summary>
    /// Serializes the session to a JSON file at the specified path.
    /// Clip audio data is not stored — only file paths and timeline positions are saved.
    /// </summary>
    /// <param name="filePath">The destination file path for the saved session JSON.</param>
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

    /// <summary>
    /// Deserializes a session from a JSON file, reloading all audio clips from their stored paths.
    /// </summary>
    /// <param name="filePath">The path to the session JSON file produced by <see cref="Save"/>.</param>
    /// <returns>A fully reconstructed <see cref="Session"/>.</returns>
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

    /// <summary>
    /// Starts a background timer that automatically saves the session to the specified file
    /// every <paramref name="intervalMinutes"/> minutes, replacing any existing auto-save timer.
    /// </summary>
    /// <param name="filePath">The file path to auto-save to.</param>
    /// <param name="intervalMinutes">How often to save, in minutes.</param>
    public void EnableAutoSave(string filePath, int intervalMinutes)
    {
        DisableAutoSave();

        _autoSaveTimer = new System.Timers.Timer(intervalMinutes * 60_000);
        _autoSaveTimer.Elapsed  += (_, _) => Save(filePath);
        _autoSaveTimer.AutoReset = true;
        _autoSaveTimer.Start();
    }

    /// <summary>Stops and removes the auto-save timer if one is active.</summary>
    public void DisableAutoSave()
    {
        _autoSaveTimer?.Stop();
        _autoSaveTimer?.Dispose();
        _autoSaveTimer = null;
    }

    /// <summary>Stops the auto-save timer and releases its resources.</summary>
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
