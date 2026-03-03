namespace Rhynohunt.Core;

/// <summary>
/// An <see cref="IEffect"/> that adds a decaying echo by mixing the original signal with a
/// delayed copy of itself. The echo repeats with each successive reflection attenuated by
/// <see cref="Feedback"/>, producing a natural reverb tail.
/// </summary>
/// <remarks>
/// The delay is calculated as <c>(int)(DelayTime.TotalSeconds * sampleRate) * 2</c> array elements,
/// where the factor of 2 accounts for the stereo interleaved layout (L, R pairs) of the input buffer.
/// </remarks>
public class DelayEffect : IEffect
{
    /// <summary>
    /// Gets or sets the time between the dry signal and its first echo.
    /// </summary>
    /// <value>Defaults to 500 ms.</value>
    public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(0.5);

    /// <summary>
    /// Gets or sets the echo decay factor. Each successive reflection is multiplied by this value.
    /// </summary>
    /// <value>Range 0.0 (no echo) to 1.0 (infinite sustain). Defaults to 0.5.</value>
    public float Feedback { get; set; } = 0.5f;

    /// <summary>
    /// Applies the delay effect to the provided stereo interleaved sample buffer.
    /// </summary>
    /// <param name="samples">The stereo interleaved input buffer.</param>
    /// <param name="sampleRate">The audio sample rate in Hz (e.g. 44100).</param>
    /// <returns>A new float array with the delay echo applied.</returns>
    public float[] Process(float[] samples, int sampleRate)
    {
        // Multiply by 2 for stereo interleaving so the delay is in wall-clock time
        int delayElements = (int)(DelayTime.TotalSeconds * sampleRate) * 2;

        float[] result = new float[samples.Length];
        Array.Copy(samples, result, samples.Length);

        // Each element reads back from itself, creating a cascading feedback echo
        for (int i = delayElements; i < result.Length; i++)
            result[i] += result[i - delayElements] * Feedback;

        return result;
    }
}
