using NAudio.Wave;
using System.ComponentModel;

namespace Rhynohunt.Core;

public class AudioClip : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public string FilePath { get; private set; }
    public float[] Samples { get; private set; }
    public int SampleRate { get; private set; }
    public int Channels { get; private set; }

    // Duration is derived from total sample count and channel layout.
    public TimeSpan Duration => TimeSpan.FromSeconds((double)Samples.Length / (SampleRate * Channels));
    
    private TimeSpan _startTime = TimeSpan.Zero;
    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime == value) return;
            _startTime = value;
            // Notify both timeline time and its pixel-mapped UI position.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftPixels)));
        }
    }
    
    public float Durvisual => (float)Duration.TotalSeconds * 15;
    public double LeftPixels => (float)StartTime.TotalSeconds * 15;

    private AudioClip(string filePath, float[] samples, int sampleRate, int channels)
    {
        FilePath = filePath;
        Samples = samples;
        SampleRate = sampleRate;
        Channels = channels;
    }

    public static AudioClip Load(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLower();

        // Route decoding by extension to keep loader behavior explicit.
        return ext switch
        {
            ".wav" => LoadWav(filePath),
            ".mp3" => LoadMp3(filePath),
            _ => throw new NotSupportedException($"File format '{ext}' is not supported. Use WAV or MP3.")
        };
    }

    private static AudioClip LoadWav(string filePath)
    {
        using AudioFileReader reader = new AudioFileReader(filePath);
        return ReadFromReader(filePath, reader);
    }

    private static AudioClip LoadMp3(string filePath)
    {
        using var mp3Reader = new NLayer.MpegFile(filePath);
        int sampleRate = mp3Reader.SampleRate;
        int channels = mp3Reader.Channels;

        List<float> samples = new List<float>();
        float[] buffer = new float[4096];
        int read;

        // NLayer returns PCM float samples already decoded from MP3 frames.
        while ((read = mp3Reader.ReadSamples(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < read; i++)
                samples.Add(buffer[i]);
        }

        return new AudioClip(filePath, samples.ToArray(), sampleRate, channels);
    }

    private static AudioClip ReadFromReader(string filePath, AudioFileReader reader)
    {
        int sampleRate = reader.WaveFormat.SampleRate;
        int channels = reader.WaveFormat.Channels;

        List<float> samples = new List<float>();
        float[] buffer = new float[4096];
        int read;

        // Read the entire file into a contiguous in-memory sample buffer.
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < read; i++)
                samples.Add(buffer[i]);
        }

        return new AudioClip(filePath, samples.ToArray(), sampleRate, channels);
    }
}
