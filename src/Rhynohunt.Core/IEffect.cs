namespace Rhynohunt.Core;

public interface IEffect
{
    float[] Process(float[] samples, int sampleRate);
}
