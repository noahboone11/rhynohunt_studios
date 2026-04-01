namespace Rhynohunt.Core;

// Uniformly scales sample amplitude.
public class GainEffect : IEffect
{
    public float GainFactor { get; set; } = 1.0f;

    // Returns a new processed buffer and leaves the input buffer unchanged.
    public float[] Process(float[] samples, int sampleRate)
    {
        float[] result = new float[samples.Length];
        for (int i = 0; i < samples.Length; i++)
            result[i] = samples[i] * GainFactor;
        return result;
    }
}
