using NAudio.Wave;

namespace Rhynohunt.Core;

/// <summary>
/// Represents an audio clip loaded from a WAV or MP3 file.
/// Stores all decoded PCM samples in memory for use by a <see cref="Track"/>.
/// </summary>
public class AudioClip
{
    /// <summary>Gets the absolute or relative file path of the source audio file.</summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// Gets the decoded PCM samples as interleaved floats in the range [-1.0, 1.0].
    /// For stereo clips the layout is [L0, R0, L1, R1, ...].
    /// </summary>
    public float[] Samples { get; private set; }

    /// <summary>Gets the sample rate of the audio clip in Hz (e.g. 44100).</summary>
    public int SampleRate { get; private set; }

    /// <summary>Gets the number of audio channels (1 = mono, 2 = stereo).</summary>
    public int Channels { get; private set; }

    /// <summary>Gets the total playback duration of the clip.</summary>
    public TimeSpan Duration => TimeSpan.FromSeconds((double)Samples.Length / (SampleRate * Channels));

    private AudioClip(string filePath, float[] samples, int sampleRate, int channels)
    {
        FilePath = filePath;
        Samples = samples;
        SampleRate = sampleRate;
        Channels = channels;
    }

    /// <summary>
    /// Loads an audio clip from the specified file path.
    /// Supports WAV and MP3 formats.
    /// </summary>
    /// <param name="filePath">The absolute or relative path to the audio file.</param>
    /// <returns>A fully decoded <see cref="AudioClip"/> ready for playback.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the file extension is not <c>.wav</c> or <c>.mp3</c>.
    /// </exception>
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
