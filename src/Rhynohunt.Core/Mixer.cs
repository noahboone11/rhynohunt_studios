namespace Rhynohunt.Core;

/// <summary>
/// Manages a collection of <see cref="Track"/> objects and renders their audio into a
/// stereo interleaved output buffer. Called by <see cref="Rhynohunt.AudioEngine.AudioEngine"/>
/// on every PortAudio buffer callback.
/// </summary>
public class Mixer
{
    private readonly List<Track> _tracks = new List<Track>();

    /// <summary>Gets the read-only list of tracks currently in this mixer.</summary>
    public IReadOnlyList<Track> Tracks => _tracks;

    /// <summary>Adds a track to the mixer.</summary>
    /// <param name="track">The track to add.</param>
    public void AddTrack(Track track) => _tracks.Add(track);

    /// <summary>Removes a track from the mixer.</summary>
    /// <param name="track">The track to remove.</param>
    public void RemoveTrack(Track track) => _tracks.Remove(track);

    /// <summary>
    /// Returns the sample rate of the first track that has a clip loaded.
    /// Falls back to 44100 Hz if no tracks have a clip loaded.
    /// </summary>
    /// <returns>The sample rate in Hz to use for playback.</returns>
    public int GetSampleRate() =>
        _tracks.FirstOrDefault(t => t.Clip != null)?.Clip?.SampleRate ?? 44100;

    /// <summary>
    /// Renders all active tracks into the provided stereo interleaved output buffer.
    /// Respects each track's gain, pan, mute, and solo settings.
    /// Output samples are clamped to [-1.0, 1.0] to prevent clipping.
    /// </summary>
    /// <param name="position">The current playback position in samples (per channel).</param>
    /// <param name="buffer">
    /// The stereo interleaved float output buffer to fill, with layout [L0, R0, L1, R1, ...].
    /// Must have at least <paramref name="frameCount"/> * 2 elements.
    /// </param>
    /// <param name="frameCount">The number of frames (sample pairs) to render.</param>
    public void Render(int position, float[] buffer, int frameCount)
    {
        // Clear buffer first
        Array.Clear(buffer, 0, frameCount * 2);

        bool anySolo = _tracks.Any(t => t.IsSolo);

        foreach (Track track in _tracks)
        {
            if (track.Clip == null) continue;
            if (track.IsMuted) continue;
            if (anySolo && !track.IsSolo) continue;

            float[] samples = track.Clip.Samples;
            int channels = track.Clip.Channels;

            // Pan law - constant power panning
            float panLeft  = (float)Math.Cos((track.Pan + 1) * Math.PI / 4);
            float panRight = (float)Math.Sin((track.Pan + 1) * Math.PI / 4);

            for (int i = 0; i < frameCount; i++)
            {
                int sampleIndex = (position + i) * channels;
                if (sampleIndex >= samples.Length) break;

                float left, right;

                if (channels == 2)
                {
                    left  = samples[sampleIndex] * track.Gain;
                    right = samples[sampleIndex + 1] * track.Gain;
                }
                else
                {
                    // Mono - duplicate to both channels
                    left  = samples[sampleIndex] * track.Gain;
                    right = left;
                }

                buffer[i * 2]     += left  * panLeft;
                buffer[i * 2 + 1] += right * panRight;
            }
        }

        // Clamp to prevent clipping
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = Math.Clamp(buffer[i], -1.0f, 1.0f);
    }
}
