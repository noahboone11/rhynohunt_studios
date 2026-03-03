using Rhynohunt.Core;

namespace Rhynohunt.AudioEngine;

/// <summary>
/// High-level playback controller that owns a <see cref="Core.Mixer"/> and an <see cref="AudioEngine"/>.
/// Provides transport controls (play, pause, stop, seek), exposes timeline properties, and fires
/// events for UI or other subscribers to react to state and time changes.
/// Implements <see cref="IDisposable"/> to release underlying resources.
/// </summary>
public class TransportController : IDisposable
{
    private readonly Mixer _mixer;
    private readonly AudioEngine _engine;
    private readonly System.Timers.Timer _timer;

    /// <summary>Gets the <see cref="Core.Mixer"/> managed by this controller. Add tracks here to configure playback.</summary>
    public Mixer Mixer => _mixer;

    /// <summary>Gets a value indicating whether the transport is currently playing.</summary>
    public bool IsPlaying => _engine.IsPlaying;

    /// <summary>Gets the current playback position as a <see cref="TimeSpan"/>.</summary>
    public TimeSpan CurrentTime =>
        TimeSpan.FromSeconds((double)_engine.Position / _mixer.GetSampleRate());

    /// <summary>
    /// Gets the total duration of the session — the end time of the last clip across all tracks.
    /// Returns <see cref="TimeSpan.Zero"/> if no clips are loaded.
    /// </summary>
    public TimeSpan TotalTime =>
        _mixer.Tracks
            .SelectMany(t => t.Clips)
            .Select(c => c.StartTime + c.Duration)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Max();

    /// <summary>
    /// Fired whenever the playback state changes (play started, paused, or stopped).
    /// Subscribers are invoked on a thread-pool thread.
    /// </summary>
    public event Action? PlaybackStateChanged;

    /// <summary>
    /// Fired every 100 ms while the transport is playing, to notify subscribers of the updated
    /// <see cref="CurrentTime"/>. Raised by an internal <see cref="System.Timers.Timer"/> —
    /// not from the audio callback.
    /// </summary>
    public event Action? TimeChanged;

    /// <summary>
    /// Initializes a new <see cref="TransportController"/> for the specified PortAudio output device.
    /// </summary>
    /// <param name="outputDevice">The PortAudio device index to use for output.</param>
    public TransportController(int outputDevice)
    {
        _mixer  = new Mixer();
        _engine = new AudioEngine(_mixer, outputDevice);

        _timer = new System.Timers.Timer(100);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    /// <summary>
    /// Starts playback from the current position. Does nothing if already playing.
    /// Fires <see cref="PlaybackStateChanged"/> after starting.
    /// </summary>
    public void Play()
    {
        if (_engine.IsPlaying) return;
        _engine.Play();
        _timer.Start();
        PlaybackStateChanged?.Invoke();
    }

    /// <summary>
    /// Pauses playback, preserving the current position for resumption.
    /// Does nothing if not currently playing.
    /// Fires <see cref="PlaybackStateChanged"/> after pausing.
    /// </summary>
    public void Pause()
    {
        if (!_engine.IsPlaying) return;
        _timer.Stop();
        _engine.Pause();
        PlaybackStateChanged?.Invoke();
    }

    /// <summary>
    /// Stops playback and resets the position to 0.
    /// Fires <see cref="PlaybackStateChanged"/> and <see cref="TimeChanged"/> after stopping.
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
        _engine.Stop();
        PlaybackStateChanged?.Invoke();
        TimeChanged?.Invoke();
    }

    /// <summary>
    /// Seeks the playback position to the specified time.
    /// Safe to call while playing or paused.
    /// Fires <see cref="TimeChanged"/> after seeking.
    /// </summary>
    /// <param name="position">The timeline position to seek to.</param>
    public void Seek(TimeSpan position)
    {
        int samplePosition = (int)(position.TotalSeconds * _mixer.GetSampleRate());
        _engine.Seek(samplePosition);
        TimeChanged?.Invoke();
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        TimeChanged?.Invoke();
    }

    /// <summary>
    /// Releases the underlying <see cref="AudioEngine"/> and stops the internal timer.
    /// </summary>
    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
        _engine.Dispose();
    }
}
