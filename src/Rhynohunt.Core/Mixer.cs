namespace Rhynohunt.Core;

public class Mixer
{
    private readonly List<Track> _tracks = new List<Track>();

    public IReadOnlyList<Track> Tracks => _tracks;

    public void AddTrack(Track track) => _tracks.Add(track);
    public void RemoveTrack(Track track) => _tracks.Remove(track);

    // Falls back to 44100 if no clips are loaded yet
    public int GetSampleRate() =>
        _tracks.SelectMany(t => t.Clips).FirstOrDefault()?.SampleRate ?? 44100;

    public void Render(int position, float[] buffer, int frameCount)
    {
        Array.Clear(buffer, 0, frameCount * 2);

        bool anySolo = _tracks.Any(t => t.IsSolo);
        int sampleRate = GetSampleRate();

        foreach (Track track in _tracks)
        {
            if (!track.Clips.Any()) continue;
            if (track.IsMuted) continue;
            if (anySolo && !track.IsSolo) continue;

            // Render this track's clips into a private stereo buffer so effects
            // can be applied before the result is mixed into the master output.
            float[] trackBuffer = new float[frameCount * 2];

            float panLeft  = (float)Math.Cos((track.Pan + 1) * Math.PI / 4);
            float panRight = (float)Math.Sin((track.Pan + 1) * Math.PI / 4);

            foreach (AudioClip clip in track.Clips)
            {
                // Convert StartTime to a frame offset for this clip
                int clipStartFrame = (int)(clip.StartTime.TotalSeconds * clip.SampleRate);
                float[] samples = clip.Samples;
                int channels = clip.Channels;

                for (int i = 0; i < frameCount; i++)
                {
                    int localFrame = (position + i) - clipStartFrame;
                    if (localFrame < 0) continue;   // clip has not started yet

                    int sampleIndex = localFrame * channels;
                    if (sampleIndex >= samples.Length) break;  // clip is exhausted

                    float left, right;
                    if (channels == 2)
                    {
                        left  = samples[sampleIndex]     * track.Gain;
                        right = samples[sampleIndex + 1] * track.Gain;
                    }
                    else
                    {
                        // Mono — duplicate to both channels
                        left  = samples[sampleIndex] * track.Gain;
                        right = left;
                    }

                    trackBuffer[i * 2]     += left  * panLeft;
                    trackBuffer[i * 2 + 1] += right * panRight;
                }
            }

            // Apply per-track effects (stereo interleaved) after gain and pan
            float[] effected = trackBuffer;
            foreach (IEffect effect in track.Effects)
                effected = effect.Process(effected, sampleRate);

            // Mix effected track into master output
            for (int i = 0; i < effected.Length; i++)
                buffer[i] += effected[i];
        }

        // Clamp to prevent clipping
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = Math.Clamp(buffer[i], -1.0f, 1.0f);
    }
}
