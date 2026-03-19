using Rhynohunt.Core;

namespace Rhynohunt.AudioEngine;

public class TransportController : IDisposable
{
    private readonly Mixer _mixer;
    private readonly AudioEngine _engine;
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

    public TransportController(int outputDevice)
    {
        _mixer  = new Mixer();
        _engine = new AudioEngine(_mixer, outputDevice);

        _timer = new System.Timers.Timer(100);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public void Play()
    {
        if (_engine.IsPlaying) return;
        _engine.Play();
        _timer.Start();
        PlaybackStateChanged?.Invoke();
    }

    public void Pause()
    {
        if (!_engine.IsPlaying) return;
        _timer.Stop();
        _engine.Pause();
        PlaybackStateChanged?.Invoke();
    }

    public void Stop()
    {
        _timer.Stop();
        _engine.Stop();
        PlaybackStateChanged?.Invoke();
        TimeChanged?.Invoke();
    }

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

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
        _engine.Dispose();
    }
}
