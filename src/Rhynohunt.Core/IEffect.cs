namespace Rhynohunt.Core;

// Contract for per-track audio effects on interleaved sample buffers.
public interface IEffect
{
    // sampleRate is supplied for time-based effects (delay, modulation, etc.).
    float[] Process(float[] samples, int sampleRate);
}
