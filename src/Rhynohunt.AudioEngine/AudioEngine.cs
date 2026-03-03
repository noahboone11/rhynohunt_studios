using PortAudioSharp;
using Rhynohunt.Core;

namespace Rhynohunt.AudioEngine;

/// <summary>
/// Wraps PortAudio to drive real-time playback of a <see cref="Mixer"/>.
/// Supports play, pause (position preserved), and stop (position reset).
/// Implements <see cref="IDisposable"/> to release the PortAudio stream.
/// </summary>
public class AudioEngine : IDisposable
{
    private readonly Mixer _mixer;
    private PortAudioSharp.Stream? _stream;
    private int _position = 0;
    private bool _isPlaying = false;
    private readonly int _outputDevice;
    private readonly double _outputLatency;

    /// <summary>Gets a value indicating whether the engine is currently playing.</summary>
    public bool IsPlaying => _isPlaying;

    /// <summary>Gets the current playback position in samples (per channel).</summary>
    public int Position => _position;

    /// <summary>
    /// Initializes a new <see cref="AudioEngine"/> for the specified mixer and output device.
    /// Queries the device's default low-output latency at construction time.
    /// </summary>
    /// <param name="mixer">The <see cref="Mixer"/> that will supply audio data.</param>
    /// <param name="outputDevice">The PortAudio device index to use for output.</param>
    public AudioEngine(Mixer mixer, int outputDevice)
    {
        _mixer = mixer;
        _outputDevice = outputDevice;

        PortAudio.Initialize();
        _outputLatency = PortAudio.GetDeviceInfo(outputDevice).defaultLowOutputLatency;
        PortAudio.Terminate();
    }

    /// <summary>
    /// Starts playback from the current position.
    /// Does nothing if the engine is already playing.
    /// Opens and starts a stereo Float32 PortAudio stream with 256 frames per buffer.
    /// The sample rate is determined dynamically by <see cref="Mixer.GetSampleRate"/>.
    /// </summary>
    public void Play()
    {
        if (_isPlaying) return;

        PortAudio.Initialize();

        _stream = new PortAudioSharp.Stream(
            inParams: null,
            outParams: new StreamParameters
            {
                device = _outputDevice,
                channelCount = 2,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = _outputLatency
            },
            sampleRate: _mixer.GetSampleRate(),
            framesPerBuffer: 256,
            streamFlags: StreamFlags.ClipOff,
            callback: OnAudioCallback,
            userData: IntPtr.Zero
        );

        _stream.Start();
        _isPlaying = true;
        Console.WriteLine("Playback started.");
    }

    /// <summary>
    /// Pauses playback, preserving the current position so playback can be resumed with <see cref="Play"/>.
    /// Does nothing if the engine is not currently playing.
    /// </summary>
    public void Pause()
    {
        if (!_isPlaying) return;
        _stream?.Stop();
        _stream?.Dispose();
        _stream = null;
        _isPlaying = false;
        PortAudio.Terminate();
        Console.WriteLine($"Paused at position {_position}.");
    }

    /// <summary>
    /// Stops playback and resets the playback position to 0.
    /// Safe to call whether or not the engine is currently playing.
    /// </summary>
    public void Stop()
    {
        if (_stream != null)
        {
            _stream.Stop();
            _stream.Dispose();
            _stream = null;
        }
        _position = 0;
        _isPlaying = false;
        PortAudio.Terminate();
        Console.WriteLine("Stopped and reset to position 0.");
    }

    private StreamCallbackResult OnAudioCallback(
        IntPtr input, IntPtr output, uint frameCount,
        ref StreamCallbackTimeInfo timeInfo,
        StreamCallbackFlags statusFlags,
        IntPtr userData)
    {
        float[] buffer = new float[frameCount * 2];
        _mixer.Render(_position, buffer, (int)frameCount);

        unsafe
        {
            float* out_ = (float*)output;
            for (int i = 0; i < buffer.Length; i++)
                out_[i] = buffer[i];
        }

        _position += (int)frameCount;
        return StreamCallbackResult.Continue;
    }

    /// <summary>
    /// Sets the playback position to the specified sample offset (per channel).
    /// Safe to call while playing or paused.
    /// </summary>
    /// <param name="samplePosition">The sample position to seek to (per channel).</param>
    public void Seek(int samplePosition)
    {
        _position = samplePosition;
    }

    /// <summary>
    /// Releases the PortAudio stream if it is open.
    /// Note: does not call <c>PortAudio.Terminate()</c> — callers should ensure
    /// <see cref="Stop"/> or <see cref="Pause"/> is called before disposing to avoid resource leaks.
    /// </summary>
    public void Dispose()
    {
        if (_stream != null)
        {
            _stream.Stop();
            _stream.Dispose();
            _stream = null;
        }
    }

    /// <summary>
    /// Searches all PortAudio output devices for one whose name contains the given substring (case-insensitive).
    /// </summary>
    /// <param name="nameContains">A substring to match against device names.</param>
    /// <returns>The device index of the first matching output device.</returns>
    /// <exception cref="Exception">Thrown when no matching output device is found.</exception>
    public static int FindOutputDevice(string nameContains)
    {
        PortAudio.Initialize();
        int deviceCount = PortAudio.DeviceCount;

        for (int i = 0; i < deviceCount; i++)
        {
            DeviceInfo info = PortAudio.GetDeviceInfo(i);
            if (info.maxOutputChannels > 0 && info.name.Contains(nameContains, StringComparison.OrdinalIgnoreCase))
            {
                PortAudio.Terminate();
                return i;
            }
        }

        PortAudio.Terminate();
        throw new Exception($"No output device found containing '{nameContains}'");
    }

    /// <summary>Returns the PortAudio default output device index.</summary>
    /// <returns>The index of the system's default output device.</returns>
    public static int DefaultOutputDevice()
    {
        PortAudio.Initialize();
        int index = PortAudio.DefaultOutputDevice;
        PortAudio.Terminate();
        return index;
    }
}
