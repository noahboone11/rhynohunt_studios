using System.Diagnostics;
using NAudio.Wave;

namespace Rhynohunt.Core;

public static class AudioExporter
{
    // Render in fixed blocks to avoid very large temporary working buffers.
    private const int ChunkFrames = 4096;

    // Mixes the full session and writes a timestamped WAV file in outputPath.
    public static void ExportWav(Session session, string outputPath)
    {
        var (mixer, totalFrames, sampleRate) = PrepareRender(session);
        if (totalFrames == 0) return;
        Directory.CreateDirectory(outputPath);
        string fileName = $"beat_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string mypath = Path.Combine(outputPath, fileName);
        float[] fullBuffer = RenderFull(mixer, totalFrames);
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
        using var writer = new WaveFileWriter(mypath, waveFormat);
        writer.WriteSamples(fullBuffer, 0, fullBuffer.Length);
    }

    // Requires ffmpeg or lame on PATH (macOS: brew install ffmpeg).
    // Encodes by rendering WAV first, then converting that file to MP3.
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

    // Builds a mixer snapshot and computes total timeline length in frames.
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

    // Renders the entire session into one interleaved stereo float buffer.
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

    // Tries ffmpeg first, then lame; succeeds on first zero exit code.
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
