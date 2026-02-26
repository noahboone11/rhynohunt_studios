using PortAudioSharp;
using Rhynohunt.Core;

namespace Rhynohunt.AudioEngine;

public class AudioEngine : IDisposable
{
    private readonly Mixer _mixer;
    private PortAudioSharp.Stream? _stream;
    private int _position = 0;
    private bool _isPlaying = false;
    private readonly int _outputDevice;
    private readonly double _outputLatency;

    public bool IsPlaying => _isPlaying;
    public int Position => _position;

    public AudioEngine(Mixer mixer, int outputDevice)
    {
        _mixer = mixer;
        _outputDevice = outputDevice;

        PortAudio.Initialize();
        _outputLatency = PortAudio.GetDeviceInfo(outputDevice).defaultLowOutputLatency;
        PortAudio.Terminate();
    }

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
            sampleRate: 44100,
            framesPerBuffer: 256,
            streamFlags: StreamFlags.ClipOff,
            callback: OnAudioCallback,
            userData: IntPtr.Zero
        );

        _stream.Start();
        _isPlaying = true;
        Console.WriteLine("Playback started.");
    }

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

    public void Dispose()
    {
        if (_stream != null)
        {
            _stream.Stop();
            _stream.Dispose();
            _stream = null;
        }
    }

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

    public static int DefaultOutputDevice()
    {
        PortAudio.Initialize();
        int index = PortAudio.DefaultOutputDevice;
        PortAudio.Terminate();
        return index;
    }
}

