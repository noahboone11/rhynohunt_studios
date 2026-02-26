namespace Rhynohunt.Core;

public class Mixer
{
    private readonly List<Track> _tracks = new List<Track>();
    public IReadOnlyList<Track> Tracks => _tracks;

    public void AddTrack(Track track) => _tracks.Add(track);
    public void RemoveTrack(Track track) => _tracks.Remove(track);

    // Called by AudioEngine on every buffer callback
    // position = current sample position in playback
    // buffer = stereo interleaved float output [L, R, L, R, ...]
    // frameCount = number of frames to fill
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