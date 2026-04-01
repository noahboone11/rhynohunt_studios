using Rhynohunt.Core;

namespace Rhynohunt.AudioEngine;

public class TransportController : IDisposable
{
    private readonly Mixer _mixer;
    private readonly AudioEngine _engine;
    // UI/update heartbeat so current time can refresh while playing.
    private readonly System.Timers.Timer _timer;

    public Mixer Mixer => _mixer;
    public bool IsPlaying => _engine.IsPlaying;

    public TimeSpan CurrentTime =>
        TimeSpan.FromSeconds((double)_engine.Position / _mixer.GetSampleRate());

    public TimeSpan TotalTime =>
        _mixer.Tracks
            .SelectMany(t => t.Clips)
            .Select(c => c.StartTime + c.Duration)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Max();

    public event Action? PlaybackStateChanged;
    public event Action? TimeChanged;

    // Creates a transport with its own mixer and output engine.
    public TransportController(int outputDevice)
    {
        _mixer  = new Mixer();
        _engine = new AudioEngine(_mixer, outputDevice);

        _timer = new System.Timers.Timer(100);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    // Starts playback and begins periodic time-change notifications.
    public void Play()
    {
        if (_engine.IsPlaying) return;
        _engine.Play();
        _timer.Start();
        PlaybackStateChanged?.Invoke();
    }

    // Pauses playback but keeps current playhead position.
    public void Pause()
    {
        if (!_engine.IsPlaying) return;
        _timer.Stop();
        _engine.Pause();
        PlaybackStateChanged?.Invoke();
    }

    // Stops playback and resets playhead to the start.
    public void Stop()
    {
        _timer.Stop();
        _engine.Stop();
        PlaybackStateChanged?.Invoke();
        TimeChanged?.Invoke();
    }

    // Converts UI time into frame position used by the engine.
    public void Seek(TimeSpan position)
    {
        int samplePosition = (int)(position.TotalSeconds * _mixer.GetSampleRate());
        _engine.Seek(samplePosition);
        TimeChanged?.Invoke();
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        int totalFrames = (int)(TotalTime.TotalSeconds * _mixer.GetSampleRate());
        // Loop back to the start when playback reaches the session end.
        if (totalFrames > 0 && _engine.Position >= totalFrames)
            
            _engine.Seek(0);

        TimeChanged?.Invoke();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
        _engine.Dispose();
    }
}
