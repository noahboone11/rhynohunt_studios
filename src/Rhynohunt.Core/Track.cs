namespace Rhynohunt.Core;
using System.Collections.ObjectModel;

public class Track
{
    private readonly ObservableCollection<AudioClip> _clips = new();
    private readonly List<IEffect> _effects = new List<IEffect>();

    public string Name { get; set; }
    public ObservableCollection<AudioClip> Clips => _clips;
    public IReadOnlyList<IEffect> Effects => _effects;

    public float Gain { get; set; } = 1.0f;
    public float Pan { get; set; } = 0.0f;
    public bool IsMuted { get; set; } = false;
    public bool IsSolo { get; set; } = false;

    public Track(string name)
    {
        Name = name;
    }

    public void AddClip(AudioClip clip, TimeSpan startTime)
    {
        clip.StartTime = startTime;
        _clips.Add(clip);
    }

    public void RemoveClip(AudioClip clip) => _clips.Remove(clip);

    public bool HasClips => _clips.Count > 0;

    public void AddEffect(IEffect effect) => _effects.Add(effect);
    public void RemoveEffect(IEffect effect) => _effects.Remove(effect);
}
