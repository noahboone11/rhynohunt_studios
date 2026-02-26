using NAudio.Wave;

namespace Rhynohunt.Core;

public class AudioClip
{
    public string FilePath { get; private set; }
    public float[] Samples { get; private set; }
    public int SampleRate { get; private set; }
    public int Channels { get; private set; }
    public TimeSpan Duration => TimeSpan.FromSeconds((double)Samples.Length / (SampleRate * Channels));

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

        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < read; i++)
                samples.Add(buffer[i]);
        }

        return new AudioClip(filePath, samples.ToArray(), sampleRate, channels);
    }
}