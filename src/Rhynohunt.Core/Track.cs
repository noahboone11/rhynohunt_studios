namespace Rhynohunt.Core;

public class Track
{
    public string Name { get; set; }
    public AudioClip? Clip { get; private set; }
    public float Gain { get; set; } = 1.0f;      // 0.0 = silent, 1.0 = full volume
    public float Pan { get; set; } = 0.0f;        // -1.0 = full left, 0.0 = center, 1.0 = full right
    public bool IsMuted { get; set; } = false;
    public bool IsSolo { get; set; } = false;

    public Track(string name)
    {
        Name = name;
    }

    public void LoadClip(AudioClip clip)
    {
        Clip = clip;
    }

    public void ClearClip()
    {
        Clip = null;
    }

    public bool HasClip => Clip != null;
}