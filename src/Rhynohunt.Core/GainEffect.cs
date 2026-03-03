namespace Rhynohunt.Core;

/// <summary>
/// An <see cref="IEffect"/> that multiplies every sample by a configurable gain factor,
/// allowing per-effect amplification or attenuation independent of the track's master gain.
/// </summary>
public class GainEffect : IEffect
{
    /// <summary>
    /// Gets or sets the gain multiplier applied to all samples.
    /// </summary>
    /// <value>1.0 leaves the signal unchanged; 0.0 silences it; values above 1.0 amplify.</value>
    public float GainFactor { get; set; } = 1.0f;

    /// <summary>
    /// Multiplies every element in <paramref name="samples"/> by <see cref="GainFactor"/>.
    /// </summary>
    /// <param name="samples">The input sample buffer.</param>
    /// <param name="sampleRate">Not used by this effect.</param>
    /// <returns>A new float array with the gain applied.</returns>
    public float[] Process(float[] samples, int sampleRate)
    {
        float[] result = new float[samples.Length];
        for (int i = 0; i < samples.Length; i++)
            result[i] = samples[i] * GainFactor;
        return result;
    }
}
