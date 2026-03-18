using System.Diagnostics;
using NAudio.Wave;

namespace Rhynohunt.Core;

/// <summary>
/// Renders a <see cref="Session"/> to an audio file on disk.
/// The full timeline is mixed offline using <see cref="Mixer.Render"/> and written in a
/// single pass — no audio device is required.
/// </summary>
public static class AudioExporter
{
    private const int ChunkFrames = 4096;

    /// <summary>
    /// Renders the entire session mix and writes it to a 32-bit IEEE float stereo WAV file.
    /// </summary>
    /// <param name="session">The session to export.</param>
    /// <param name="outputPath">The destination file path for the WAV file.</param>
    public static void ExportWav(Session session, string outputPath)
    {
        var (mixer, totalFrames, sampleRate) = PrepareRender(session);
        if (totalFrames == 0) return;

        float[] fullBuffer = RenderFull(mixer, totalFrames);

        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
        using var writer = new WaveFileWriter(outputPath, waveFormat);
        writer.WriteSamples(fullBuffer, 0, fullBuffer.Length);
    }

    /// <summary>
    /// Renders the entire session mix and encodes it as an MP3 file.
    /// Requires <c>ffmpeg</c> or <c>lame</c> to be installed and on the system PATH.
    /// On macOS: <c>brew install ffmpeg</c>.  On Windows/Linux: install ffmpeg from ffmpeg.org.
    /// </summary>
    /// <param name="session">The session to export.</param>
    /// <param name="outputPath">The destination file path for the MP3 file.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when neither <c>ffmpeg</c> nor <c>lame</c> is available on the PATH.
    /// </exception>
    public static void ExportMp3(Session session, string outputPath)
    {
        // Render to a temp WAV first, then encode with a CLI tool
        string tempWav = Path.ChangeExtension(outputPath, ".tmp.wav");
        try
        {
            ExportWav(session, tempWav);
            EncodeMp3WithCli(tempWav, outputPath);
        }
        finally
        {
            if (File.Exists(tempWav)) File.Delete(tempWav);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="Mixer"/> from the session's tracks and calculates the total
    /// frame count and sample rate needed for rendering.
    /// </summary>
    private static (Mixer mixer, int totalFrames, int sampleRate) PrepareRender(Session session)
    {
        var mixer = new Mixer();
        foreach (var track in session._tracks)
            mixer.AddTrack(track);

        int sampleRate = mixer.GetSampleRate();

        TimeSpan totalTime = session._tracks
            .SelectMany(t => t.Clips)
            .Select(c => c.StartTime + c.Duration)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Max();

        int totalFrames = (int)(totalTime.TotalSeconds * sampleRate);
        return (mixer, totalFrames, sampleRate);
    }

    /// <summary>
    /// Renders the entire timeline into a single stereo interleaved float buffer using
    /// fixed-size chunks to avoid large per-frame allocations.
    /// </summary>
    private static float[] RenderFull(Mixer mixer, int totalFrames)
    {
        float[] fullBuffer = new float[totalFrames * 2];
        float[] chunk = new float[ChunkFrames * 2];

        for (int pos = 0; pos < totalFrames; pos += ChunkFrames)
        {
            int frames = Math.Min(ChunkFrames, totalFrames - pos);
            mixer.Render(pos, chunk, frames);
            Array.Copy(chunk, 0, fullBuffer, pos * 2, frames * 2);
        }

        return fullBuffer;
    }

    /// <summary>
    /// Encodes a WAV file to MP3 by invoking <c>ffmpeg</c> or <c>lame</c> as a subprocess.
    /// Tries ffmpeg first, then lame. Throws if neither is found.
    /// </summary>
    private static void EncodeMp3WithCli(string inputWav, string outputMp3)
    {
        var candidates = new (string tool, string args)[]
        {
            ("ffmpeg", $"-y -i \"{inputWav}\" -codec:a libmp3lame -q:a 2 \"{outputMp3}\""),
            ("lame",   $"\"{inputWav}\" \"{outputMp3}\"")
        };

        foreach (var (tool, args) in candidates)
        {
            try
            {
                var psi = new ProcessStartInfo(tool, args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false
                };

                using var proc = Process.Start(psi)!;
                proc.WaitForExit();

                if (proc.ExitCode == 0) return;
            }
            catch (Exception)
            {
                // Tool not found — try the next one
            }
        }

        throw new InvalidOperationException(
            "MP3 export requires 'ffmpeg' or 'lame' on the system PATH. " +
            "On macOS: brew install ffmpeg");
    }
}
