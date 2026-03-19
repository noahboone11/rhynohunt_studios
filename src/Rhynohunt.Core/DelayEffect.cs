namespace Rhynohunt.Core;

public class DelayEffect : IEffect
{
    public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(0.5);
    public float Feedback { get; set; } = 0.5f;

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
