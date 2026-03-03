namespace Rhynohunt.Core;

/// <summary>
/// Defines a single audio processing effect that transforms a buffer of PCM samples.
/// Effects are applied per-track in <see cref="Mixer.Render"/> after gain and pan have been applied.
/// The input buffer is always stereo interleaved (layout: L0, R0, L1, R1, ...).
/// </summary>
public interface IEffect
{
    /// <summary>
    /// Processes the provided sample buffer and returns the effected result.
    /// The returned array may be a new allocation or the same array modified in place.
    /// </summary>
    /// <param name="samples">
    /// Stereo interleaved float samples in the range [-1.0, 1.0].
    /// </param>
    /// <param name="sampleRate">The audio sample rate in Hz (e.g. 44100).</param>
    /// <returns>The processed sample buffer, same length as <paramref name="samples"/>.</returns>
    float[] Process(float[] samples, int sampleRate);
}
